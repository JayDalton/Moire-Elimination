#pragma once

#include <array>
#include <algorithm>
#include <string>
#include <vector>
#include <gdcmReader.h>
#include <gdcmWriter.h>
#include <gdcmImageReader.h>

class DicomHelper
{
private:
	gdcm::Reader reader;
	gdcm::Writer writer;

	bool ConvertToFormat_RGB888(gdcm::Image const & gimage, char *buffer);

public:
	DicomHelper();
	~DicomHelper();

	std::vector<double> GetFileData(const std::string filePath);

	void SetFileData(const std::string filePath, std::vector<double> data);
};

