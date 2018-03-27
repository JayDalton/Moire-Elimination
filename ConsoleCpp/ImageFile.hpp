#pragma once

#include <filesystem>

#include "AppTypes.hpp"
#include "ImageMemory.hpp"


class ImageFile
{

public:
	ImageFile() = delete;
	ImageFile(const std::string& name) : fileName(name) {};

	auto GetRows() const { return rows; }
	auto GetCols() const { return cols; }

	bool CanRead() const;
	bool CanWrite() const;

	template<class PixelType> auto ReadFromFileSystem(std::size_t rows, std::size_t cols);
	template<class PixelType> auto WriteToFileSystem(const std::vector<PixelType>& collection);

	//template<class PixelType> auto ReadFromMemory();
	//template<class PixelType> auto WriteToMemory();

private:
	std::string fileName;
	std::size_t rows = 4320;
	std::size_t cols = 4318;
};

bool ImageFile::CanRead() const
{
	namespace fs = std::experimental::filesystem;
	return (fs::exists(fileName) && fs::is_regular_file(fileName));
}

bool ImageFile::CanWrite() const
{
	return true;
}

template<class PixelType>
auto ImageFile::WriteToFileSystem(const std::vector<PixelType>& content)
{
	std::ofstream file(fileName, std::ios::binary);
	file.write(reinterpret_cast<const char*>(content.data()), content.size() * sizeof(PixelType));
}

template<class PixelType>
auto ImageFile::ReadFromFileSystem(std::size_t rows, std::size_t cols)
{
	std::ifstream file(fileName, std::ios::binary);

	// Stop eating new lines in binary mode!!!
	file.unsetf(std::ios::skipws);

	file.seekg(0, std::ios::end);
	auto fileSize = file.tellg();
	file.seekg(0, std::ios::beg);

	auto length = fileSize / sizeof(PixelType);

	std::vector<PixelType> content;

	if (0 == (fileSize % sizeof(PixelType)) && rows * cols == length)
	{
		content.resize(length);
		file.read(reinterpret_cast<char*>(content.data()), content.size() * sizeof(PixelType));
	}

	return content;
}

