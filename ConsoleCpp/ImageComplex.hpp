#pragma once

#include <algorithm>
#include <vector>
#include <complex>
#include <numeric>
#include <iterator>
#include <iostream>
#include <valarray>
#include <cmath>
#include <cassert>

#include "AppTypes.hpp"
#include "NumberIterator.hpp"
#include "ImageFile.hpp"

using ImageFilePtr = std::unique_ptr<ImageFile>;

class ImageComplex {
public:
	ImageComplex() = default;

	auto ReadFromImageFileOld(ImageFilePtr file);
	auto WriteToImageFileOld(ImageFilePtr file);
	auto ReadFromImageFileNew(ImageFilePtr file);
	auto WriteToImageFileNew(ImageFilePtr file);

	auto& GetLineByRow(const std::size_t row) const;

	auto TransformFourier(std::size_t row, bool back);

	auto GetNumberOfRows() const { return rows; }
	auto GetNumberOfColumns() const { return cols; }

private:
	ComplexImage complexImage;
	ComplexMatrix complexMatrix;
	ComplexMatrix fourierMatrix;

	const float PI = 3.14159265358979323846;

	std::size_t rows{ 0 };
	std::size_t cols{ 0 };
};

auto ImageComplex::TransformFourier(std::size_t row, bool back = false)
{
	auto& s = complexMatrix.at(row);
	ComplexImage result(s.begin(), s.begin());

	const float pol{ 2.0f * PI * (back ? -1.0f : 1.0f) };
	const float div{ back ? 1.0f : float(s.size()) };

	auto sum_up([=, &s](std::size_t j) {
		return[=, &s](Complex32 c, std::size_t k) {
			return c + s[k] * std::polar<float>(1.0f, pol * k * j / float(s.size()));
		};
	});

	auto to_ft([=, &s](std::size_t j) 
	{
		return std::accumulate(
			num_iterator{ 0 },
			num_iterator( s.size() ), 
			Complex32{},
			sum_up(j))
		/ div;
	});

	std::transform(num_iterator{ 0 }, num_iterator{ s.size() }, std::begin(result), to_ft);

	fourierMatrix.insert({row, result});
	return result;
}

auto& ImageComplex::GetLineByRow(const std::size_t row) const
{
	return complexMatrix.at(row);
}

auto ImageComplex::ReadFromImageFileNew(ImageFilePtr file)
{
	rows = file->GetRows();
	cols = file->GetCols();
	auto content = file->ReadFromFileSystem<Pixel16>(rows, cols);
	assert(rows * cols == content.size());

	constexpr PixelF32 scale = 1.0f / std::numeric_limits<Pixel16>::max();

	for (size_t row = 0; row < rows; row++)
	{
		ComplexImage complexLine;
		complexLine.reserve(cols);

		auto rangeStart = std::begin(content) + (row * cols);
		auto rangeEnd = std::begin(content) + (row * cols) + cols;

		std::transform(rangeStart, rangeEnd, std::back_inserter(complexLine),
			[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
		);

		complexMatrix.insert({ row, complexLine });
	}

	return file;
}

auto ImageComplex::ReadFromImageFileOld(ImageFilePtr file)
{
	auto rows{ file->GetRows() };
	auto cols{ file->GetCols() };
	auto content = file->ReadFromFileSystem<Pixel16>(rows, cols);

	complexImage.reserve(content.size());
	constexpr PixelF32 scale = 1.0f / std::numeric_limits<Pixel16>::max();

	auto start = std::chrono::high_resolution_clock::now();
	{
		std::transform(std::begin(content), std::end(content), std::back_inserter(complexImage),
			[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
		);
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;
	std::cout << "ReadFromImageFile: " << diff.count() << std::endl;

	return file;
}

auto ImageComplex::WriteToImageFileNew(ImageFilePtr file)
{
	PixelCollection<Pixel16> result;
	result.reserve(complexMatrix.size() * complexMatrix.size());
	constexpr PixelF32 scale = std::numeric_limits<Pixel16>::max();

	for (const auto& [row, line] : complexMatrix)
	{
		std::transform(std::begin(line), std::end(line), std::back_inserter(result),
			[scale](Complex32 c) -> Pixel16 {return static_cast<Pixel16>(c.real() * scale); }
		);
	}

	file->WriteToFileSystem<Pixel16>(result);

	return file;
}

auto ImageComplex::WriteToImageFileOld(ImageFilePtr file)
{
	auto content{ std::vector<Pixel16>() };

	content.reserve(complexImage.size());
	constexpr PixelF32 scale = std::numeric_limits<Pixel16>::max();

	auto start = std::chrono::high_resolution_clock::now();
	{
		std::transform(std::begin(complexImage), std::end(complexImage), std::back_inserter(content),
			[scale](Complex32 c) -> Pixel16 {return static_cast<Pixel16>(c.real() * scale); }
		);
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;
	std::cout << "WriteToImageFile: " << diff.count() << std::endl;

	file->WriteToFileSystem<Pixel16>(content);

	return file;
}

