// ConsoleCpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <chrono>
#include <fstream>
#include <iostream>
#include <algorithm>
#include <filesystem>

#include "DicomHelper.h"

namespace fs = std::experimental::filesystem;

int main()
{
	std::string path("c:\\Develop\\DICOM\\BilderRaw");
	if (fs::exists(path))
	{
		for (auto& p : fs::directory_iterator(path))
		{
			if (fs::is_regular_file(p)) 
			{
				std::ifstream file(p, std::ios::binary);
				//file.seekg(0, std::ios_base::end);
				//unsigned file_len = file.tellg();
				//unsigned short buff[file_len];

				if (file.is_open())
				{
					std::vector<short> vs;
					vs.insert(vs.begin(), std::istream_iterator<short>(file), std::istream_iterator<short>());

					//vs.insert(std::begin(vs), std::istream_iterator<short>(file), std::istream_iterator<short>());
					//file.read(reinterpret_cast<char*>(&buff), sizeof(buff));
				}

			}
		}
	}

	DicomHelper helper;



    return 0;
}

