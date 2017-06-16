#include <opencv2/core.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>
#include <iostream>

#include <iostream>
#include <fstream>

using namespace cv;
using namespace std;

struct PixelValues {
	double AdftComplexRe;
	double AdftComplexIm;
	double Bmagnitude;
	double Cscalar;
	double Dlogarithm;
};

static void help(char* progName)
{
	std::cout << std::endl
		<< "This program demonstrated the use of the discrete Fourier transform (DFT). " << std::endl
		<< "The dft of an image is taken and it's power spectrum is displayed." << std::endl
		<< "Usage:" << std::endl
		<< progName << " [image_name -- default Assets/0001.jpg] " << std::endl << std::endl;
}

void takeDFT(cv::Mat& source, cv::Mat& target) 
{
	cv::Mat originalComplex[2] = { source, cv::Mat::zeros(source.size(), CV_32F) };

	cv::Mat dftReady;

	cv::merge(originalComplex, 2, dftReady);

	cv::Mat dftOfOriginal;

	cv::dft(dftReady, dftOfOriginal, cv::DFT_COMPLEX_OUTPUT);

	target = dftOfOriginal;
}

void recenterDFT(cv::Mat& source) 
{
	int centerX = source.cols / 2;
	int centerY = source.rows / 2;

	cv::Mat q1(source, cv::Rect(0, 0, centerX, centerY));
	cv::Mat q2(source, cv::Rect(centerX, 0, centerX, centerY));
	cv::Mat q3(source, cv::Rect(0, centerY, centerX, centerY));
	cv::Mat q4(source, cv::Rect(centerX, centerY, centerX, centerY));

	cv::Mat swapMap;

	q1.copyTo(swapMap);
	q4.copyTo(q1);
	swapMap.copyTo(q4);

	q2.copyTo(swapMap);
	q3.copyTo(q2);
	swapMap.copyTo(q3);
}

void showDFT(std::string label, cv::Mat& source) 
{
	cv::Mat splitArray[2] = {cv::Mat::zeros(source.size(), CV_32F), cv::Mat::zeros(source.size(), CV_32F) };

	cv::split(source, splitArray);

	cv::Mat dftMagnitude;

	cv::magnitude(splitArray[0], splitArray[1], dftMagnitude);

	dftMagnitude += cv::Scalar::all(1);

	cv::log(dftMagnitude, dftMagnitude);

	cv::normalize(dftMagnitude, dftMagnitude, 0, 1, CV_MINMAX);

	cv::imshow(label, dftMagnitude);

	cv::waitKey();
}

void invertDFT(cv::Mat& source, cv::Mat& target) 
{
	cv::Mat inverse;

	cv::dft(source, inverse, cv::DFT_INVERSE | cv::DFT_REAL_OUTPUT | cv::DFT_SCALE);

	target = inverse;
}

void createGaussian(cv::Size& size, cv::Mat& output, int uX, int uY, float sigmaX, float sigmaY, float amplitude = 1.0) 
{
	cv::Mat temp = cv::Mat(size, CV_32F);

	for (int r = 0; r < size.height; r++)
	{
		for (int c = 0; c < size.width; c++)
		{
			float x = ((c - uX) * ((float)c - uX)) / (2.0f * sigmaX * sigmaX);
			float y = ((r - uY) * ((float)r - uY)) / (2.0f * sigmaY * sigmaY);
			float value = amplitude * exp(-(x + y));
			temp.at<float>(r, c) = value;
		}
	}
	cv::normalize(temp, temp, 0.0f, 1.0f, cv::NORM_MINMAX);
	output = temp;
}

void showMagImage(cv::Mat& image)
{
	// rearrange the quadrants of Fourier image  so that the origin is at the image center
	int cx = image.cols / 2;
	int cy = image.rows / 2;

	Mat q0(image, Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
	Mat q1(image, Rect(cx, 0, cx, cy));  // Top-Right
	Mat q2(image, Rect(0, cy, cx, cy));  // Bottom-Left
	Mat q3(image, Rect(cx, cy, cx, cy)); // Bottom-Right

	Mat tmp;                           // swap quadrants (Top-Left with Bottom-Right)
	q0.copyTo(tmp);
	q3.copyTo(q0);
	tmp.copyTo(q3);

	q1.copyTo(tmp);                    // swap quadrant (Top-Right with Bottom-Left)
	q2.copyTo(q1);
	tmp.copyTo(q2);

	cv::normalize(image, image, 0, 1, NORM_MINMAX); // Transform the matrix with float values into a
													 // viewable image form (float between values 0 and 1).

	cv::imshow("spectrum magnitude", image);
}

void write2ChannelToString(cv::Mat& image, std::ostringstream& oss1, std::ostringstream& oss2)
{
	CV_DbgAssert(image.depth() == CV_32F);	// float
	CV_DbgAssert(image.channels() == 2);	// 1 - grey, 2 - complex, 3 - rgb

	for (int row = 0; row < image.rows; ++row)
	{
		float* p = image.ptr<float>(row);
		for (int col = 0; col < image.cols * image.channels(); ++col)
		{
			if (0 < col) oss1 << ";";
			if (0 < col) oss2 << ";";
			oss1 << p[col++];
			oss2 << p[col];
		}
		oss1 << std::endl;
		oss2 << std::endl;
	}
}

void write2ChannelLogFile(cv::Mat& image, const char* outfile)
{
	CV_DbgAssert(image.depth() == CV_32F);	// float
	CV_DbgAssert(image.channels() == 2);	// 1 - grey, 2 - complex, 3 - rgb

	std::string fname(outfile);
	std::ofstream channel1(fname + std::string("Channel1.log"), std::ios::binary);
	std::ofstream channel2(fname + std::string("Channel2.log"), std::ios::binary);
	if (!channel1.is_open() || !channel2.is_open())
	{
		std::cout << "Can not open outfile: " << outfile << std::endl;
		return;
	}

	double t = (double)getTickCount();

	std::ostringstream oss1, oss2;
	write2ChannelToString(image, oss1, oss2);
	channel1 << oss1.str();
	channel2 << oss2.str();

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "Write 2 Channel LogFile in seconds: " << t << endl;
}

void write1ChannelToString(cv::Mat& image, std::ostringstream& oss) 
{
	CV_DbgAssert(image.depth() == CV_32F);	// float
	CV_DbgAssert(image.channels() == 1);	// 1 - grey, 2 - complex, 3 - rgb

	for (int row = 0; row < image.rows; ++row)
	{
		float* p = image.ptr<float>(row);
		for (int col = 0; col < image.cols; ++col)
		{
			if (0 < col) oss << ";";
			oss << p[col];
		}
		oss << std::endl;
	}
}

void write1ChannelLogFile(cv::Mat& image, const char* outfile)
{
	CV_DbgAssert(image.depth() == CV_32F);	// float
	CV_DbgAssert(image.channels() == 1);	// 1 - grey, 2 - complex, 3 - rgb

	std::ofstream logfile(outfile, std::ios::binary);
	if (!logfile.is_open())
	{
		std::cout << "Can not open outfile: " << outfile << std::endl;
		return;
	}

	double t = (double)getTickCount();

	std::ostringstream oss;
	write1ChannelToString(image, oss);
	logfile << oss.str();

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "Write 1 Channel LogFile in seconds: " << t << endl;
}

void fullImageIterate(cv::Mat& image) 
{
	Mat paddedImage;								// expand input image to optimal size
	int m = cv::getOptimalDFTSize(image.rows);
	int n = cv::getOptimalDFTSize(image.cols);		// on the border add zero values
	cv::copyMakeBorder(image, paddedImage, 0, m - image.rows, 0, n - image.cols, BORDER_CONSTANT, Scalar::all(0));

	Mat planes[] = { Mat_<float>(paddedImage), Mat::zeros(paddedImage.size(), CV_32F) };
	Mat complexI;
	cv::merge(planes, 2, complexI);					// Add to the expanded another plane with zeros

	cv::dft(complexI, complexI);					// this way the result may fit in the source matrix

	//write2ChannelLogFile(complexI, "c:/Temp/Complex");

													// compute the magnitude and switch to logarithmic scale
	Mat magI;										// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
	cv::split(complexI, planes);					// planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))
	cv::magnitude(planes[0], planes[1], magI);		// planes[0] = magnitude

	//write1ChannelLogFile(magI, "c:/Temp/magnitude.log");

	magI += Scalar::all(1);							// switch to logarithmic scale
	cv::log(magI, magI);

	//write1ChannelLogFile(magI, "c:/Temp/logarithmic.log");

	magI = magI(Rect(0, 0, magI.cols & -2, magI.rows & -2));	// crop the spectrum, if it has an odd number of rows or columns

	//showMagImage(magI);
	// rearrange the quadrants of Fourier image  so that the origin is at the image center
	int cx = magI.cols / 2;
	int cy = magI.rows / 2;
	
	cv::Mat q0(magI, cv::Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
	cv::Mat q1(magI, cv::Rect(cx, 0, cx, cy));  // Top-Right
	cv::Mat q2(magI, cv::Rect(0, cy, cx, cy));  // Bottom-Left
	cv::Mat q3(magI, cv::Rect(cx, cy, cx, cy)); // Bottom-Right
	
	cv::Mat tmp;                           // swap quadrants (Top-Left with Bottom-Right)
	q0.copyTo(tmp);
	q3.copyTo(q0);
	tmp.copyTo(q3);
	
	q1.copyTo(tmp);                    // swap quadrant (Top-Right with Bottom-Left)
	q2.copyTo(q1);
	tmp.copyTo(q2);
	
	normalize(magI, magI, 0, 1, cv::NORM_MINMAX); // Transform the matrix with float values into a
												// viewable image form (float between values 0 and 1).
	
	imshow("Input Image", image);    // Show the result
	imshow("spectrum magnitude", magI);
	cv::waitKey();

	// Wait until user press some key
	waitKey(0);
}

void linewiseIterate(cv::Mat& image) 
{
	double t = (double)getTickCount();

	std::ostringstream oss;
	for (int row = 0; row < image.rows; ++row)
	{
		cv::Mat one_row = image.row(row);

		Mat paddedRow;									// expand input image to optimal size
		int n = cv::getOptimalDFTSize(one_row.cols);	// on the border add zero values
		cv::copyMakeBorder(one_row, paddedRow, 0, 0, 0, n - image.cols, BORDER_CONSTANT, Scalar::all(0));

		Mat planes[] = { Mat_<float>(paddedRow), Mat::zeros(paddedRow.size(), CV_32F) };
		Mat complexI;
		cv::merge(planes, 2, complexI);					// Add to the expanded another plane with zeros

		cv::dft(complexI, complexI);

		Mat magI;										// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
		cv::split(complexI, planes);					// planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))
		cv::magnitude(planes[0], planes[1], magI);		// planes[0] = magnitude

		magI += Scalar::all(1);							// switch to logarithmic scale
		cv::log(magI, magI);

		write1ChannelToString(magI, oss);

		magI = magI(Rect(0, 0, magI.cols & -2, magI.rows & -2));	// crop the spectrum, if it has an odd number of rows or columns
	}

	std::string fname("C:/Temp/LineWiseImage.log");
	std::ofstream logfile(fname, std::ios::binary);
	if (!logfile.is_open())
	{
		std::cout << "Can not open outfile: " << fname << std::endl;
		return;
	}
	logfile << oss.str();

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "Linewise iterate in seconds: " << t << endl;
}

void showRawImage() 
{
	unsigned short IMG_ROWS = 4320;
	unsigned short IMG_COLS = 4318;
	const char* filename = "c:/Develop/DICOM/Bilder/PE_Image.raw";

	std::ifstream fin(filename, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = fin.tellg();
	std::vector<unsigned short> result(pos / 2);	// 16 bits
	fin.seekg(0, std::ios::beg);
	fin.read(reinterpret_cast<char*>(&result[0]), pos);
	fin.close();

	if (result.size() == IMG_ROWS * IMG_COLS)
	{
		cv::Mat image(IMG_ROWS, IMG_COLS, CV_16U, &result[0]);
		cv::imshow("RAW Image", image);

		cv::Mat target;
		cv::resize(image, target, cv::Size(600, 600), 0, 0, INTER_AREA);
		//cv::cvtColor(target, target, CV_GRAY2RGB);// CV_BayerBG2GRAY
		cv::imshow("Resize Image", target);
		cv::waitKey();
	}
}

void showDcmImage() 
{
	//DicomImage DCM_image("test.dcm");

	//DcmFileFormat file;
	unsigned short IMG_ROWS = 4320;
	unsigned short IMG_COLS = 4318;
	const char* filename = "c:/Develop/DICOM/Bilder/PE_Image.dcm";
}

int main(int argc, char ** argv)
{
	help(argv[0]);

	//showRawImage();

	const char* filename = argc >= 2 ? argv[1] : "Assets/0001.jpg";

	Mat I = imread(filename, IMREAD_GRAYSCALE);
	if (I.empty()) return -1;

	//linewiseIterate(I);
	fullImageIterate(I);

	waitKey();

	return 0;
}


//int main(int argc, char ** argv)
//{
//	help(argv[0]);
//
//	const char* filename = argc >= 2 ? argv[1] : "Assets/imageTextN.png";
//
//	cv::Mat I = cv::imread(filename, cv::IMREAD_GRAYSCALE);
//	if (I.empty()) { return -1; }
//
//	cv::Mat padded;                            //expand input image to optimal size
//	int m = cv::getOptimalDFTSize(I.rows);
//	int n = cv::getOptimalDFTSize(I.cols); // on the border add zero values
//	cv::copyMakeBorder(I, padded, 0, m - I.rows, 0, n - I.cols, cv::BORDER_CONSTANT, cv::Scalar::all(0));
//
//	cv::Mat planes[] = { cv::Mat_<float>(padded), cv::Mat::zeros(padded.size(), CV_32F) };
//	cv::Mat complexI;
//	cv::merge(planes, 2, complexI);         // Add to the expanded another plane with zeros
//
//	cv::dft(complexI, complexI);            // this way the result may fit in the source matrix
//
//										// compute the magnitude and switch to logarithmic scale
//										// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
//	cv::split(complexI, planes);                   // planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))
//	cv::magnitude(planes[0], planes[1], planes[0]);// planes[0] = magnitude
//	cv::Mat magI = planes[0];
//
//	magI += cv::Scalar::all(1);                    // switch to logarithmic scale
//	cv::log(magI, magI);
//
//	// crop the spectrum, if it has an odd number of rows or columns
//	magI = magI(cv::Rect(0, 0, magI.cols & -2, magI.rows & -2));
//
//	// rearrange the quadrants of Fourier image  so that the origin is at the image center
//	int cx = magI.cols / 2;
//	int cy = magI.rows / 2;
//
//	cv::Mat q0(magI, cv::Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
//	cv::Mat q1(magI, cv::Rect(cx, 0, cx, cy));  // Top-Right
//	cv::Mat q2(magI, cv::Rect(0, cy, cx, cy));  // Bottom-Left
//	cv::Mat q3(magI, cv::Rect(cx, cy, cx, cy)); // Bottom-Right
//
//	cv::Mat tmp;                           // swap quadrants (Top-Left with Bottom-Right)
//	q0.copyTo(tmp);
//	q3.copyTo(q0);
//	tmp.copyTo(q3);
//
//	q1.copyTo(tmp);                    // swap quadrant (Top-Right with Bottom-Left)
//	q2.copyTo(q1);
//	tmp.copyTo(q2);
//
//	normalize(magI, magI, 0, 1, cv::NORM_MINMAX); // Transform the matrix with float values into a
//											  // viewable image form (float between values 0 and 1).
//
//	imshow("Input Image", I);    // Show the result
//	imshow("spectrum magnitude", magI);
//	cv::waitKey();
//
//	return 0;
//}
