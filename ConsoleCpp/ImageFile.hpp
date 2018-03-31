#pragma once

#include <cassert>
#include <filesystem>

#include "AppTypes.hpp"

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

	file.seekg(0, std::ios::end);
	auto fileSize = file.tellg();
	file.seekg(0, std::ios::beg);

	assert(0 == fileSize % sizeof(PixelType));
	std::size_t number = fileSize / sizeof(PixelType);
	assert(rows * cols == number);

	PixelCollection<PixelType> content(number);
	file.read(reinterpret_cast<char*>(content.data()), content.size() * sizeof(PixelType));

	return content;
}

