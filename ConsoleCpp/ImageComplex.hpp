#pragma once

#include <algorithm>
#include <vector>
#include <complex>
#include <numeric>
#include <iterator>
#include <iostream>
#include <valarray>
#include <cmath>


#include "AppTypes.hpp"
#include "ImageMemory.hpp"

using ImageFilePtr = std::unique_ptr<ImageFile>;
using ImageComplexLine = ComplexCollection<Complex32>;
using ImageComplexMatrix = std::vector<ImageComplexLine>;

class ImageComplex {
public:
	ImageComplex() = default;
	auto ReadFromImageFile(ImageFilePtr file);
	auto WriteToImageFile(ImageFilePtr file);
	auto TransformFourier(std::size_t row, ImageComplexLine& line, bool back);

	auto TransformLineById(const std::size_t const row, const ImageComplexLine& line);

private:
	ImageComplexLine imageComplexLine;
	ImageComplexMatrix complexImageMatrix;

	const float PI = 3.14159265358979323846;
};

auto ImageComplex::TransformLineById(const std::size_t const row, const ImageComplexLine& line)
{
}

auto ImageComplex::TransformFourier(std::size_t row, ImageComplexLine& line, bool back = false)
{
	auto s = imageComplexLine;

	ImageComplexLine t(s.begin(), s.begin());

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
			num_iterator(0), 
			num_iterator( s.size() ), 
			Complex32{},
			sum_up(j))
		/ div;
	});

	std::transform(num_iterator{ 0 }, num_iterator{ s.size() }, std::begin(t), to_ft);

	complexImageMatrix.push_back(t);
	return t;
}

auto ImageComplex::ReadFromImageFile(ImageFilePtr file)
{
	auto rows{ file->GetRows() };
	auto cols{ file->GetCols() };
	auto content = file->ReadFromFileSystem<Pixel16>(rows, cols);

	imageComplexLine.reserve(content.size());
	constexpr PixelF32 scale = 1.0f / std::numeric_limits<Pixel16>::max();

	auto start = std::chrono::high_resolution_clock::now();
	{
		std::transform(std::begin(content), std::end(content), std::back_inserter(imageComplexLine),
			[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
		);
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;
	std::cout << "ReadFromImageFile: " << diff.count() << std::endl;

	return file;
}

auto ImageComplex::WriteToImageFile(ImageFilePtr file)
{
	auto content{ std::vector<Pixel16>() };

	content.reserve(imageComplexLine.size());
	constexpr PixelF32 scale = std::numeric_limits<Pixel16>::max();

	auto start = std::chrono::high_resolution_clock::now();
	{
		std::transform(std::begin(imageComplexLine), std::end(imageComplexLine), std::back_inserter(content),
			[scale](Complex32 c) -> Pixel16 {return static_cast<Pixel16>(c.real() * scale); }
		);
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;
	std::cout << "WriteToImageFile: " << diff.count() << std::endl;

	file->WriteToFileSystem<Pixel16>(content);

	return file;
}

