// ConsoleCpp.cpp : Defines the entry point for the console application.
//

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

using ImageFilePtr = std::unique_ptr<ImageFile>;

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
		importFile = imageComplex->ReadFromImageFile(std::move(importFile));

		exportFile = imageComplex->WriteToImageFile(std::move(exportFile));

		//auto imageFile = std::make_unique<ImageFile>(importName);
				
		//auto content = importFile->ReadFileContent<Pixel16>(rows, cols);

		//ImageMemory image{ content, rows, cols };

		//std::cout << "Image: " << image.getNumberOfPixel() << std::endl;

		//image.Transform();

		//imageComplex->ReadContent(image);
		//exportFile->SaveFileContent<Pixel16>();
		//importFile->SaveFileContent<Pixel16>(exportFile);
	}

    return 0;
}

