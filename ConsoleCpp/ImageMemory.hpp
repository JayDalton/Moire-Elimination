#pragma once

#include <algorithm>
#include <vector>

#include "AppTypes.hpp"
#include "ImageFile.hpp"

class ImageMemory {
public:
	ImageMemory() = delete;
	ImageMemory(const PixelCollection<Pixel16>& image, const std::size_t rows, const std::size_t cols) : _rows(rows), _cols(cols), _data(image) {}

	std::size_t getNumberOfRows() const { return _rows; }
	std::size_t getNumberOfColumns() const { return _cols; }
	std::size_t getNumberOfPixel() const { return _rows * _cols; }

	PixelCollection<Pixel16> getPixelCollection() const { return _data; }

	auto Transform();

private:
	std::size_t _rows;
	std::size_t _cols;
	PixelCollection<Pixel16> _data;
	PixelCollection<PixelF32> _floats;
	PixelCollection<Complex32> _complex;
};

auto ImageMemory::Transform()
{
	PixelF32 scale = 1.0f / std::numeric_limits<Pixel16>::max();
	PixelCollection<PixelF32> result;
	//std::transform(std::begin(_data), std::end(_data), std::back_inserter(result),
	//	SaturateTo<PixelF32>(scale)
	//);

	auto start = std::chrono::high_resolution_clock::now();
	{
		// action
	}
	auto end = std::chrono::high_resolution_clock::now();
	std::chrono::duration<double> diff = end - start;

	auto x = diff.count();

	std::transform(std::begin(_data), std::end(_data), std::back_inserter(result),
		[scale](Pixel16 p) -> PixelF32 { return saturate_cast<PixelF32>(p * scale); }
	);

	std::transform(std::begin(_data), std::end(_data), std::back_inserter(_complex),
		[scale](Pixel16 p) -> Complex32 { return Complex32(p * scale); }
	);

}