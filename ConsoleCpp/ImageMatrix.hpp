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
#include "ImageFile.hpp"
#include "ImageComplex.hpp"

using ImageComplexPtr = std::unique_ptr<ImageComplex>;

class ImageMatrix
{
public:
	ImageMatrix() = default;
	auto ReadFromImageFile(ImageComplexPtr image);
	auto WriteToImageFile(ImageComplexPtr image);

private:
	ComplexMatrix complexMatrix;

};

auto ImageMatrix::ReadFromImageFile(ImageComplexPtr image)
{
	image->GetNumberOfRows();
}

auto ImageMatrix::WriteToImageFile(ImageComplexPtr image)
{

}
