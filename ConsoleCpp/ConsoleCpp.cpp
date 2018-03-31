#include "stdafx.h"

#include <array>
#include <chrono>
#include <vector>
#include <fstream>
#include <complex>
#include <iostream>
#include <algorithm>
#include <filesystem>

#include "AppTypes.hpp"
#include "ImageFile.hpp"
#include "ImageMemory.hpp"
#include "ImageComplex.hpp"

namespace fs = std::experimental::filesystem;

int main()
{

	std::string importName{ "c:\\Develop\\DICOM\\BilderRaw\\Tisch1.le.raw" };
	std::string exportName{ importName + ".out" };

	auto importFile{ std::make_unique<ImageFile>(importName) };
	auto exportFile{ std::make_unique<ImageFile>(exportName) };

	if (importFile->CanRead())	// valid file
	{
		std::cout << "file: " << importName << std::endl;
		auto imageComplex{ std::make_unique<ImageComplex>() };

		auto start = std::chrono::high_resolution_clock::now();
		{
			importFile = imageComplex->ReadFromImageFileNew(std::move(importFile));
		}
		auto end = std::chrono::high_resolution_clock::now();
		std::chrono::duration<double> diff = end - start;
		std::cout << "ReadFromImageFile: " << diff.count() << std::endl;

		// transform forward
		start = std::chrono::high_resolution_clock::now();
		{
			//imageComplex->TransformFourier(0, false);
		}
		end = std::chrono::high_resolution_clock::now();
		diff = end - start;
		std::cout << "TransformFourier forward: " << diff.count() << std::endl;

		// filter 

		// transform back

		start = std::chrono::high_resolution_clock::now();
		{
			exportFile = imageComplex->WriteToImageFileNew(std::move(exportFile));
		}
		end = std::chrono::high_resolution_clock::now();
		diff = end - start;
		std::cout << "WriteToImageFile: " << diff.count() << std::endl;
	}

    return 0;
}

