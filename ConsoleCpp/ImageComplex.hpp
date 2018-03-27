#pragma once

#include <algorithm>
#include <vector>
#include <complex>

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
	auto ReadContent(const ImageMemory& const image);

private:
	auto transformFourier();
	ImageComplexLine imageComplexLine;
	ImageComplexMatrix complexImageMatrix;

	//const std::unique_ptr<ImageFile> imageFile;
	const double PI = 3.14159265358979323846;
};

auto ImageComplex::transformFourier()
{
	auto start = std::chrono::high_resolution_clock::now();
	{
		// action
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;

}

auto ImageComplex::ReadFromImageFile(ImageFilePtr file)
{
	auto rows{ file->GetRows() };
	auto cols{ file->GetCols() };
	auto content = file->ReadFromFileSystem<Pixel16>(rows, cols);

	imageComplexLine.reserve(content.size());
	PixelF32 scale = 1.0f / std::numeric_limits<Pixel16>::max();

	std::transform(std::begin(content), std::end(content), std::back_inserter(imageComplexLine),
		[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
	);

	return file;
}

auto ImageComplex::WriteToImageFile(ImageFilePtr file)
{
	auto content{ std::vector<Pixel16>() };

	content.reserve(imageComplexLine.size());
	PixelF32 scale = std::numeric_limits<Pixel16>::max();

	std::transform(std::begin(imageComplexLine), std::end(imageComplexLine), std::back_inserter(content),
		[scale](Complex32 c) -> Pixel16 {return static_cast<Pixel16>(c.real() * scale); }
	);

	file->WriteToFileSystem<Pixel16>(content);

	return file;
}

auto ImageComplex::ReadContent(const ImageMemory& const image) 
{
	complexImageMatrix.clear();
	complexImageMatrix.reserve(image.getNumberOfRows());

	//std::transform(std::begin(image), std::end(image), std::back_inserter(_complex),
	//	[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
	//);

	auto size = image.getNumberOfColumns();
	auto data = image.getPixelCollection();

	ImageComplexLine icl{ data.begin(), data.end() };


	for (auto row = 0; row < image.getNumberOfRows(); row++)
	{
		auto a = icl.begin();
		const double pol{-2.0 * PI / size };

		ImageComplexLine signal(size);
		for (size_t i{ 0 }; i < size; i++)
		{
			for (size_t j = 0; j < size; j++)
			{


			}
		}
	}


	for (auto row = 0; row < image.getNumberOfRows(); row++)
	{
		auto start = data.begin() + (row * size);
		ImageComplexLine cc{ start, start + size};
		complexImageMatrix.push_back(cc);
	}

	return true;
}

