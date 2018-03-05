#include "stdafx.h"
#include "DicomHelper.h"


DicomHelper::DicomHelper()
{
}


std::vector<double> DicomHelper::GetFileData(const std::string filename)
{
	reader.SetFileName(filename.c_str());
	if (!reader.Read())
	{
		std::cerr << "Could not read: " << filename << std::endl;
		//return nullptr;
	}

	// The output of gdcm::Reader is a gdcm::File
	gdcm::File &file = reader.GetFile();

	// the dataset is the the set of element we are interested in:
	gdcm::DataSet &ds = file.GetDataSet();

	gdcm::ImageReader ir;
	ir.SetFileName(filename.c_str());
	if (!ir.Read())
	{
		//Read failed
		//return 1;
	}

	const gdcm::Image &gimage = ir.GetImage();
	std::vector<char> vbuffer;
	vbuffer.resize(gimage.GetBufferLength());
	char *buffer = &vbuffer[0];

	if (ConvertToFormat_RGB888(gimage, buffer))
	{

	}

	return std::vector<double>();
}

bool DicomHelper::ConvertToFormat_RGB888(gdcm::Image const & gimage, char *buffer)
{
	const unsigned int* dimension = gimage.GetDimensions();

	unsigned int dimX = dimension[0];
	unsigned int dimY = dimension[1];

	gimage.GetBuffer(buffer);

	if (gimage.GetPhotometricInterpretation() == gdcm::PhotometricInterpretation::MONOCHROME2)
	{
		if (gimage.GetPixelFormat() == gdcm::PixelFormat::UINT8)
		{
			// We need to copy each individual 8bits into R / G and B:
			unsigned char *ubuffer = new unsigned char[dimX*dimY * 3];
			unsigned char *pubuffer = ubuffer;
			for (unsigned int i = 0; i < dimX*dimY; i++)
			{
				*pubuffer++ = *buffer;
				*pubuffer++ = *buffer;
				*pubuffer++ = *buffer++;
			}

			//imageQt = new QImage(ubuffer, dimX, dimY, QImage::Format_RGB888);
		}
		else if (gimage.GetPixelFormat() == gdcm::PixelFormat::INT16)
		{
			// We need to copy each individual 16bits into R / G and B (truncate value)
			short *buffer16 = (short*)buffer;
			unsigned char *ubuffer = new unsigned char[dimX*dimY * 3];
			unsigned char *pubuffer = ubuffer;
			for (unsigned int i = 0; i < dimX*dimY; i++)
			{
				// Scalar Range of gdcmData/012345.002.050.dcm is [0,192], we could simply do:
				// *pubuffer++ = *buffer16;
				// *pubuffer++ = *buffer16;
				// *pubuffer++ = *buffer16;
				// instead do it right:
				*pubuffer++ = (unsigned char)std::min(255, (32768 + *buffer16) / 255);
				*pubuffer++ = (unsigned char)std::min(255, (32768 + *buffer16) / 255);
				*pubuffer++ = (unsigned char)std::min(255, (32768 + *buffer16) / 255);
				buffer16++;
			}

			//imageQt = new QImage(ubuffer, dimX, dimY, QImage::Format_RGB888);
		}
		else
		{
			std::cerr << "Pixel Format is: " << gimage.GetPixelFormat() << std::endl;
			return false;
		}
	}
	else
	{
		std::cerr << "Unhandled PhotometricInterpretation: " << gimage.GetPhotometricInterpretation() << std::endl;
		return false;
	}

	return true;
}
