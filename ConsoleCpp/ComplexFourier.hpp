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

class ComplexFourier
{
public:
	ComplexFourier() = default;
	auto ReadFromImageComplex(ImageComplexPtr image);
	auto WriteToImageComplex(ImageComplexPtr image);

private:
	ComplexMatrix complexImageMatrix;

};

auto ComplexFourier::ReadFromImageComplex(ImageComplexPtr image)
{

}

auto ComplexFourier::WriteToImageComplex(ImageComplexPtr image)
{

}
