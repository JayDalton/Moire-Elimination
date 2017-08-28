#include <opencv2/core.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>

#include <iostream>
#include <fstream>
#include <climits>
#include <cfloat>
#include <intrin.h>
#include <cmath>
#include <math.h>
#include <cfenv>

using namespace cv;
using namespace std;

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

void invertDFT(cv::Mat& source, cv::Mat& target) 
{
	cv::Mat inverse;

	cv::dft(source, inverse, cv::DFT_INVERSE | cv::DFT_REAL_OUTPUT | cv::DFT_SCALE);

	target = inverse;
}

double pixelDistance(double u, double v)
{
	return cv::sqrt(u*u + v*v);
}

double gaussianCoeff(double u, double v, double d0)
{
	double d = pixelDistance(u, v);
	return 1.0 - cv::exp((-d*d) / (2 * d0*d0));
}

cv::Mat create1dGaussianCurve(cv::Mat& source)
{
	cv::Mat result (source.rows, source.cols, CV_32F);

	//std::vector<float> temp;

	float sigma = 30.0, mu = 750.0;		// magic

	float t1 = 1.0 / (sigma * std::sqrtf(2.0 * CV_PI));
	float min = FLT_MAX, max = FLT_MIN;

	auto p = result.ptr<float>(0);
	for (size_t i = 0; i < result.cols / 2; i++)
	{
		float bu = 1 - std::exp(- 0.5f * (((i - mu) / sigma) * ((i - mu) / sigma)));

		//float t2 = (-((i - mu) * (i - mu))) / (2.0 * sigma * sigma);
		//float bu = t1 - t1 * std::exp(t2);

		//temp.push_back(bu);
		p[i] = bu;
		p[result.cols - i-1] = bu;

		if (bu < min) { min = bu; }
		if (max < bu) { max = bu; }
	}

	float offset = 1.0 - max;
	for (size_t i = 0; i < result.cols; i++)
	{
		p[i] += offset;
	}

	return result;
}

cv::Mat createGaussianHighPassFilter(cv::Size size, float cutoffInPixels)
{
	Mat ghpf(size, CV_32F);

	cv::Point center(size.width / 2, size.height / 2);

	for (int u = 0; u < ghpf.rows; u++)
	{
		for (int v = 0; v < ghpf.cols; v++)
		{
			ghpf.at<float>(u, v) = gaussianCoeff(u - center.y, v - center.x, cutoffInPixels);
		}
	}

	return ghpf;
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

// create a 2-channel butterworth low-pass filter with radius D, order n
// (assumes pre-aollocated size of dft_Filter specifies dimensions)
void createButterworthLowpassFilter(Mat &dft_Filter, int D, int n)
{
	Mat tmp = Mat(dft_Filter.rows, dft_Filter.cols, CV_32F);

	Point centre = Point(dft_Filter.rows / 2, dft_Filter.cols / 2);
	double radius;

	for (int i = 0; i < dft_Filter.rows; i++)
	{
		for (int j = 0; j < dft_Filter.cols; j++)
		{
			radius = (double)sqrt(pow((i - centre.x), 2.0) + pow((double)(j - centre.y), 2.0));
			tmp.at<float>(i, j) = (float)(1 / (1 + pow((double)(radius / D), (double)(2 * n))));
		}
	}

	Mat toMerge[] = { tmp, tmp };
	merge(toMerge, 2, dft_Filter);
}

void createButterworthHighpassFilter(cv::Mat& dft_filter, int D, int n)
{
	Mat tmp = Mat(dft_filter.rows, dft_filter.cols, CV_32F);

	Point centre = Point(dft_filter.rows / 2, dft_filter.cols / 2);
	double radius;

	for (int i = 0; i < dft_filter.rows; i++)
	{
		for (int j = 0; j < dft_filter.cols; j++)
		{
			radius = (double)sqrt(pow((i - centre.x), 2.0) + pow((double)(j - centre.y), 2.0));
			tmp.at<float>(i, j) = (float)(1 / (1 + pow((double)(D / radius), (double)(2 * n))));
		}
	}

	Mat toMerge[] = { tmp, tmp };
	merge(toMerge, 2, dft_filter);
}

void createGaussianBandstopFilter() 
{
	cv::Mat tmp = cv::Mat(1, 1, CV_32F);
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

void performDFT(cv::Mat& source, cv::Mat& target)
{
	double t = (double)getTickCount();

	cv::Mat paddedImage;								// expand input image to optimal size
	int m = cv::getOptimalDFTSize(source.rows);
	int n = cv::getOptimalDFTSize(source.cols);		// on the border add zero values
	cv::copyMakeBorder(source, paddedImage, 0, m - source.rows, 0, n - source.cols, BORDER_CONSTANT, Scalar::all(0));

	cv::Mat planes[] = { Mat_<float>(paddedImage), Mat::zeros(paddedImage.size(), CV_32F) };

	cv::Mat complexI;
	cv::merge(planes, 2, complexI);					// Add to the expanded another plane with zeros
	cv::dft(complexI, target, DFT_COMPLEX_OUTPUT);	// this way the result may fit in the source matrix

	t = ((double)getTickCount() - t) / getTickFrequency();
	//cout << "perform DFT in seconds: " << t << endl;
}

void lineDftInvert(cv::Mat& source, cv::Mat& target)
{
	double t = (double)getTickCount();

	for (int row = 0; row < source.rows; ++row)
	{
		cv::Mat one_row = source.row(row);

		cv::Mat inverse;
		cv::dft(one_row, inverse, cv::DFT_INVERSE | cv::DFT_REAL_OUTPUT | cv::DFT_SCALE);
		target.push_back(inverse);
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "line dft invert: " << t << endl;
}

void lineDftPerfom(cv::Mat& source, cv::Mat& target) 
{
	double t = (double)getTickCount();
	for (int row = 0; row < source.rows; ++row)
	{
		cv::Mat one_row = source.row(row);

		cv::Mat paddedLine;								// expand input image to optimal size
		int m = cv::getOptimalDFTSize(one_row.rows);
		int n = cv::getOptimalDFTSize(one_row.cols);		// on the border add zero values
		cv::copyMakeBorder(one_row, paddedLine, 0, m - one_row.rows, 0, n - one_row.cols, BORDER_CONSTANT, Scalar::all(0));

		cv::Mat linePlanes[] = { Mat_<float>(paddedLine), Mat::zeros(paddedLine.size(), CV_32F) };

		cv::Mat complexLine;
		cv::merge(linePlanes, 2, complexLine);					// Add to the expanded another plane with zeros

		cv::dft(complexLine, complexLine, DFT_COMPLEX_OUTPUT);	// this way the result may fit in the source matrix

		target.push_back(complexLine);
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "line dft perform: " << t << endl;
}

void format_matrix_to_magnitude(cv::Mat& source, cv::Mat& target)
{
	double t = (double)getTickCount();

	for (int row = 0; row < source.rows; ++row)
	{
		cv::Mat dft_row = source.row(row);

		cv::Mat planes[2];
		cv::split(dft_row, planes);						// planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))

		cv::Mat magI;									// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
		cv::magnitude(planes[0], planes[1], magI);		// planes[0] = magnitude
		magI += Scalar::all(1);							// switch to logarithmic scale
		cv::log(magI, magI);
		target.push_back(magI);

		//std::stringstream ss;
		//float* p = target.ptr<float>(0);
		//for (int col = 0; col < target.cols; ++col)
		//{
		//	if (0 < col) ss << ";";
		//	ss << p[col];
		//}
		//ss << std::endl;
		//ofs << ss.str();
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved log file: " << t << endl;
}

void print_matrix_to_log_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chan = source.channels();		// 1
	if (chan != 1)
	{
		std::cout << "NOT saved! channels " << chan << std::endl;
		return;
	}

	std::ofstream ofs(filename, std::ios::binary);
	if (!ofs.is_open())
	{
		std::cout << "Can not open outfile: " << filename << std::endl;
		return;
	}

	std::stringstream ss;
	ss.precision(4);
	ss << std::fixed;
	for (int row = 0; row < source.rows; ++row)
	{
		cv::Mat dft_row = source.row(row);

		//cv::Mat planes[2];
		//cv::split(dft_row, planes);						// planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))

		//cv::Mat magI;									// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
		//cv::magnitude(planes[0], planes[1], magI);		// planes[0] = magnitude
		//magI += Scalar::all(1);							// switch to logarithmic scale
		//cv::log(magI, magI);

		std::stringstream ss;
		float* p = dft_row.ptr<float>(0);
		for (int col = 0; col < dft_row.cols; ++col)
		{
			if (0 < col) ss << ";";
			ss << p[col];
		}
		ss << std::endl;
	}
	ofs << ss.str();

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved log file: " << t << endl;
}

void save_matrix_to_pgm_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chan = source.channels();		// 1
	if (chan != 1)
	{
		std::cout << "NOT saved! channels " << chan << std::endl;
		return;
	}

	cv::Mat resultImage;
	switch (source.type())
	{
	case CV_16U:
		resultImage = source.clone();
		break;
	case CV_32F:
		source.convertTo(resultImage, CV_16UC1, USHRT_MAX);
		break;
	default:
		return;
	}

	cv::imwrite(filename, resultImage);

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "Saved pgm file: " << t << endl;
}

void save_matrix_to_float_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chan = source.channels();		// 1
	if (chan != 1)
	{
		std::cout << "NOT saved! channels " << chan << std::endl;
		return;
	}

	cv::Mat resultImage;
	switch (source.type())
	{
	case CV_16U:
		source.convertTo(resultImage, CV_32FC1, USHRT_MAX);
		break;
	case CV_32F:
		resultImage = source.clone();
		break;
	default:
		return;
	}

	std::ofstream ofs(filename, std::ofstream::binary | std::ofstream::out);
	if (!ofs.is_open())
	{
		std::cout << "Can not open image file: " << filename << std::endl;
		return;
	}

	auto chans = resultImage.channels();
	auto nRows = resultImage.rows;
	auto nCols = resultImage.cols;

	if (resultImage.isContinuous())
	{
		nCols *= nRows;
		nRows = 1;
	}

	for (int row = 0; row < nRows; ++row)
	{
		auto sz = sizeof(float);
		auto p = resultImage.ptr<float>(row);
		for (int col = 0; col < nCols; ++col)
		{
			ofs.write(reinterpret_cast<const char*>(&p[col]), sz);
		}
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved raw file: " << t << endl;
}

void save_matrix_to_format_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chans = source.channels();		// 1
	if (chans != 1)
	{
		std::cout << "NOT saved! channels " << chans << std::endl;
		return;
	}

	auto mType = source.type();
	auto nRows = source.rows;
	auto nCols = source.cols;
	if (source.isContinuous())
	{
		nCols *= nRows;
		nRows = 1;
	}

	std::ofstream ofs(filename, std::ofstream::binary | std::ofstream::out);
	if (!ofs.is_open())
	{
		std::cout << "Can not open image file: " << filename << std::endl;
		return;
	}

	std::stringstream ss;
	if (mType == CV_16U)
	{
	}
	else if (mType == CV_32F)
	{
		ss.precision(4);
		ss << std::fixed;
	}

	for (int row = 0; row < source.rows; ++row)
	{
		//cv::Mat dft_row = source.row(row);

		//std::stringstream ss;
		float* p = source.ptr<float>(row);
		for (int col = 0; col < source.cols; ++col)
		{
			if (0 < col) ss << ";";
			ss << p[col];
		}
		ss << std::endl;
	}
	ofs << ss.str();

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved raw file: " << t << endl;
}

void save_matrix_to_binary_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chans = source.channels();		// 1
	if (chans != 1)
	{
		std::cout << "NOT saved! channels " << chans << std::endl;
		return;
	}

	auto mType = source.type();
	auto nRows = source.rows;
	auto nCols = source.cols;
	if (source.isContinuous())
	{
		nCols *= nRows;
		nRows = 1;
	}

	std::ofstream ofs(filename, std::ofstream::binary | std::ofstream::out);
	if (!ofs.is_open())
	{
		std::cout << "Can not open image file: " << filename << std::endl;
		return;
	}

	if (mType == CV_16U)
	{
		for (int row = 0; row < nRows; ++row)
		{
			auto sz = sizeof(unsigned short);
			auto p = source.ptr<unsigned short>(row);
			for (int col = 0; col < nCols; ++col)
			{
				ofs.write(reinterpret_cast<const char*>(&p[col]), sz);
			}
		}
	}
	else if (mType == CV_32F) 
	{
		for (int row = 0; row < nRows; ++row)
		{
			auto sz = sizeof(float);
			auto p = source.ptr<float>(row);
			for (int col = 0; col < nCols; ++col)
			{
				ofs.write(reinterpret_cast<const char*>(&p[col]), sz);
			}
		}
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved raw file: " << t << endl;
}

void save_matrix_to_short_file(cv::Mat& source, std::string filename)
{
	double t = (double)getTickCount();

	auto chan = source.channels();		// 1
	if (chan != 1)
	{
		std::cout << "NOT saved! channels " << chan << std::endl;
		return;
	}

	cv::Mat resultImage;
	switch (source.type())
	{
		case CV_16U:
			resultImage = source.clone();
			break;
		case CV_32F:
			source.convertTo(resultImage, CV_16UC1, USHRT_MAX);
			break;
		default:
			return;
	}

	std::ofstream ofs(filename, std::ofstream::binary | std::ofstream::out);
	if (!ofs.is_open())
	{
		std::cout << "Can not open image file: " << filename << std::endl;
		return;
	}

	auto chans = resultImage.channels();
	auto nRows = resultImage.rows;
	auto nCols = resultImage.cols;

	if (resultImage.isContinuous())
	{
		nCols *= nRows;
		nRows = 1;
	}

	for (int row = 0; row < nRows; ++row)
	{
		auto sz = sizeof(unsigned short);
		auto p = resultImage.ptr<unsigned short>(row);
		for (int col = 0; col < nCols; ++col)
		{
			ofs.write(reinterpret_cast<const char*>(&p[col]), sz);
		}
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "saved raw file: " << t << endl;
}

float calc_f_estimate(float fs, float fg)
{
	float festimate;
	if (fs >= 2 * fg)
	{
		festimate = fg;
	}
	else
	{
		float fg_fs = fg / fs;	// 8,4646 / 10.0 = 0,84646 ??? 1,1814
		float intpart;
		float fractpart = modf(fg_fs, &intpart);	// 
		float k = (fractpart < 0.5) ? floor(fg_fs) : ceil(fg_fs);	// 1
		festimate = abs(fg - k * fs);	// 8,4646 - 1 * 10,0 = 1,5354
	}
	return festimate;
}

void lineDftAbsolute(cv::Mat& source, cv::Mat& target)
{
	int nRows = source.rows;
	int nCols = source.cols;
	int channels = source.channels();
	target = source.clone();

	for (int row = 0; row < nRows; ++row)
	{
		cv::Mat dft_row = source.row(row);
		cv::Mat tar_row = target.row(row);
		auto p = dft_row.ptr<float>(0);
		auto p2 = tar_row.ptr<float>(0);

		for (int col = 0; col < nCols; col+=channels)
		{
			float re = p[col + 0];
			float im = p[col + 1];
			std::complex<float> value(re, im);

			value = std::pow(value * std::conj(value), 0.5);

			p2[col + 0] = value.real();
			p2[col + 1] = value.imag();
		}
		auto x = 0;
	}
	auto y = 0;
}

std::complex<float> calc_range_mean(const cv::Mat& row, int start, int length) 
{
	complex<float> result(0, 0);
	auto p = row.ptr<float>(0);
	for (size_t i = start; i <= start + length * 2; i += 2)
	{
		std::complex<float> test(p[i], p[i+1]);
		result += test;
	}
	return result / (float)length;
}

void lineDftFilter(cv::Mat& source, cv::Mat& target)
{
	double t = (double)getTickCount();

	float fs = 10.0;				// 10px/mm
	float fg = 215 / 2.54 * 0.1;	// 215px/inch -> ~84,646px/cm -> ~8,4646px/mm
	float festimate = calc_f_estimate(fs, fg);	// ~1,53

	cv::Mat filter = source.clone();
	//createButterworthLowpassFilter(filter, source.cols / 12, 1);
	//lineDftFilePrint(filter, "c:/Temp/ButterworthLowpassFilter.log");

	//createButterworthHighpassFilter(filter, source.cols / 12, 1);
	//lineDftFilePrint(filter, "c:/Temp/ButterworthHighpassFilter.log");

	int nRows = source.rows;
	int nCols = source.cols;
	int channels = source.channels();

	float pos = festimate / fs;
	float pos_dft = nCols * channels * pos;
	float neg_dft = nCols * channels - pos_dft;

	auto curve = create1dGaussianCurve(source.row(0));
	write1ChannelLogFile(curve, "c:/Temp/LineWiseDft1dCurve.log");

	// pos verbessern duch Suche nach Maximum nach links und rechts

	int range1_start = /*700 * channels;*/ pos_dft - (pos_dft / 5);
	int range1_close = /*800 * channels;*/ pos_dft + (pos_dft / 5);

	int range2_start = /*(nCols - 800) * channels;*/ neg_dft - (pos_dft / 5);
	int range2_close = /*(nCols - 700) * channels;*/ neg_dft + (pos_dft / 5);

	float pos_max_re = 0, pos_max_im = 0;
	bool updated = false;
	for (int row = 0; row < nRows; ++row)
	{
		cv::Mat dft_row = source.row(row);

		auto p = dft_row.ptr<float>(0);
		std::complex<float> test(p[0], p[1]);

		float mean_re = 0, mean_im = 0;
		float min_re = FLT_MAX, max_re = FLT_MIN, min_im = FLT_MAX, max_im = FLT_MIN;
		if (!updated) {
			for (size_t i = range1_start; i <= range1_close; i += 2)
			{
				float re = p[i + 0];
				float im = p[i + 1];
				if (re < min_re) { min_re = re; }
				if (max_re < re) { max_re = re; pos_max_re = i; }
				if (im < min_im) { min_im = im; }
				if (max_im < im) { max_im = im; pos_max_im = i; }
				mean_re += p[i + 0];
				mean_im += p[i + 1];
			}
			mean_re /= (range1_close - range1_start) / channels;
			mean_im /= (range1_close - range1_start) / channels;
			
			pos_dft = pos_max_im;
			neg_dft = nCols * channels - pos_dft;

			// pos verbessern duch Suche nach Maximum nach links und rechts

			range1_start = /*700 * channels;*/ pos_dft - (pos_dft / 10);
			range1_close = /*800 * channels;*/ pos_dft + (pos_dft / 10);
			range2_start = /*(nCols - 800) * channels;*/ neg_dft - (pos_dft / 10);
			range2_close = /*(nCols - 700) * channels;*/ neg_dft + (pos_dft / 10);
		}
		updated = true;

		// Irrtum - wird nicht gebraucht
		//int count = 0;
		//float sum_sqr_re = 0;
		//float sum_sqr_im = 0;

		//std::complex<float> cmean(0.0, 0.0);
		//std::complex<float> csumsqr(0.0, 0.0);
		//for (size_t i = range1_start; i <= range1_close; i += 2)
		//{
		//	float re = p[i + 0];
		//	float im = p[i + 1];
		//	std::complex<float> v(re, im);
		//	if (re < min_re) { min_re = re; }
		//	if (max_re < re) { max_re = re; pos_max_re = i; }
		//	if (im < min_im) { min_im = im; }
		//	if (max_im < im) { max_im = im; pos_max_im = i; }
		//	mean_re += p[i + 0];
		//	mean_im += p[i + 1];
		//	cmean += v;
		//	sum_sqr_re += p[i + 0] * p[i + 0];
		//	sum_sqr_im += p[i + 1] * p[i + 1];
		//	csumsqr += v * v;
		//	count++;
		//}
		//mean_re /= (range1_close - range1_start) / channels;
		//mean_im /= (range1_close - range1_start) / channels;
		//cmean /= (range1_close - range1_start) / channels;
		//// Standardabweichung
		//float mean_sqr_re = (mean_re*mean_re*count);
		//float std_dev_re = sqrt((sum_sqr_re - mean_sqr_re) / (float)count);
		//float mean_sqr_im = (mean_im*mean_im*count);
		//float std_dev_im = sqrt((sum_sqr_im - mean_sqr_im) / (float)count);
		//std::complex<float> cmeansqr = (cmean*cmean*(float)count);
		//std::complex<float> cstddev = sqrt((csumsqr - cmeansqr) / (float)count);

		// rennt durch alle werte und setzt bereich auf null
		//for (int col = 0; col <= nCols * channels; col+=2)
		//{
		//	if (range1_start <= col && col <= range1_close)
		//	{
		//		p[col + 0] = mean_re;
		//		p[col + 1] = mean_im;
		//	}

		//	if (range2_start <= col && col <= range2_close)
		//	{
		//		p[col + 0] = mean_re;
		//		p[col + 1] = mean_im;
		//	}
		//}

		// Mittelwerte links und rechts finden
		int rangeWidth = pos_dft / 10;
		std::complex<float> range1Pre = calc_range_mean(dft_row, range1_start - rangeWidth, rangeWidth);
		std::complex<float> range1Post = calc_range_mean(dft_row, range1_close, rangeWidth);
		std::complex<float> range2Pre = calc_range_mean(dft_row, range2_start - rangeWidth, rangeWidth);
		std::complex<float> range2Post = calc_range_mean(dft_row, range2_close, rangeWidth);

		// Interpolation innerhalb der Range
		cv::Mat dft_row_inter = source.row(row).clone();
		auto p_inter = dft_row_inter.ptr<float>(0);

		auto offset1 = (range1Post - range1Pre) / (float)rangeWidth;
		for (int col = range1_start, k = 0; col < range1_close; col += 2, ++k) {
			p_inter[col + 0] = range1Pre.real() + k * offset1.real();
			p_inter[col + 1] = range1Pre.imag() + k * offset1.imag();
		}

		auto offset2 = (range2Post - range2Pre) / (float)rangeWidth;
		for (int col = range2_start, k = 0; col < range2_close; col += 2, ++k) {
			p_inter[col + 0] = range2Pre.real() + k * offset2.real();
			p_inter[col + 1] = range2Pre.imag() + k * offset2.imag();
		}

		//target.push_back(dft_row_inter);

		// Gaussian band stop filter
		//std::complex<float> mean(cmean);// mean(mean_re, mean_im);
		//std::complex<float> std_dev(cstddev);// std_dev(0.1, 0.1);// (std_dev_re, std_dev_im);
		float std_dev = 1.0;
		auto pCurve = curve.ptr<float>(0);
		for (int col = 0; col < nCols * channels; col += 2) {
			std::complex<float> vcur(p[col + 0], p[col + 1]);
			std::complex<float> vcur_inter(p_inter[col + 0], p_inter[col + 1]);

			// ursprünglich ohne Interpolation
			//auto c = vcur * pCurve[col / 2];

			// neu: mit interpolierten Werten
			float x = pCurve[col / 2];
			if (x < 0.5)
			{
				float z = 1;
			}

			auto c = vcur_inter + ((vcur - vcur_inter) * x);

			//auto l = std::pow(0.5f, vcur * std::conj(vcur));
			//auto c = vcur * pCurve[col / 2];
			//auto c = std::complex<float>(x, vcur.imag());

			std::feclearexcept(FE_ALL_EXCEPT);
			//std::complex<float> v1 = 1.0f / (std_dev * (float)sqrt(2.0*CV_PI));
			//std::complex<float> v2 = vcur - mean;
			//std::complex<float> v3a = -(v2 * v2);
			//std::complex<float> v3b = (2.0f * std_dev * std_dev);
			//std::complex<float> v3 = v3a / v3b;// std::conj(v3b);
			//std::complex<float> bu1 = v1 - v1 * exp(v3);

			//std::complex<float> v4 = (vcur - mean) / std_dev;
			//std::complex<float> bu2 = 1.0f - exp(-0.5f*(v2*v2));

			//float v1 = 1.0f / (std_dev * (float)sqrt(2.0*CV_PI));
			//float v2 = col - pos_dft;
			//float v3a = -(v2 * v2);
			//float v3b = (2.0f * std_dev * std_dev);
			//float v3 = v3a / v3b;// std::conj(v3b);
			//float bu1 = v1 - v1 * exp(v3);

			//if (!std::fetestexcept(FE_INVALID)) {
			//	p[col + 0] = bu1.real();
			//	p[col + 1] = bu1.imag();
			//}
			if (!std::fetestexcept(FE_INVALID)) {
				p[col + 0] = c.real();
				p[col + 1] = c.imag();
			}
		}

		target.push_back(dft_row);
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "filter dft in seconds: " << t << endl;
}

std::vector<unsigned short> readRawImage(const char* filename, bool swap = false)
{
	double t = (double)getTickCount();

	std::ifstream fin(filename, std::ios::binary | std::ios::ate);
	std::ifstream::pos_type pos = fin.tellg();
	std::vector<unsigned short> result(pos / 2);	// 16 bits
	fin.seekg(0, std::ios::beg);
	fin.read(reinterpret_cast<char*>(&result[0]), pos);
	fin.close();

	if (swap)
	{
		// swap byte order (intrin.h)
		for (auto& item : result) {
			item = _byteswap_ushort(item);
		}
	}

	t = ((double)getTickCount() - t) / getTickFrequency();
	cout << "read raw endian: " << t << endl;

	return result;
}

void showImage(const char* label, Mat& image, cv::Size size = cv::Size()) 
{
	if (0 < size.height && 0 < size.width)
	{
		cv::Mat target;
		cv::resize(image, target, size, 0, 0, INTER_AREA);
		cv::imshow(label, target);
	}
	else
	{
		cv::imshow(label, image);
	}
	cv::waitKey();
}

void showDFT(const char* label, cv::Mat& source, cv::Size size = cv::Size(), bool rearrange = false)
{
	cv::Mat splitArray[2] = { cv::Mat::zeros(source.size(), CV_32F), cv::Mat::zeros(source.size(), CV_32F) };
	cv::split(source, splitArray);

	cv::Mat dftMagnitude;
	cv::magnitude(splitArray[0], splitArray[1], dftMagnitude);
	dftMagnitude += cv::Scalar::all(1);

	cv::log(dftMagnitude, dftMagnitude);
	cv::normalize(dftMagnitude, dftMagnitude, 0, 1, CV_MINMAX);

	if (rearrange)
	{
		recenterDFT(dftMagnitude);
	}

	showImage(label, dftMagnitude, size);
}

int main(int argc, char ** argv)
{
	cv::Size screen(600, 600);
	unsigned short IMG_ROWS = 4320;
	unsigned short IMG_COLS = 4318;
	std::string inputName("c:/Develop/DICOM/BilderDcm/Tisch3.dcm.raw");

	// read file to short array
	auto data = readRawImage(inputName.c_str(), false);

	// load, show data matrix
	cv::Mat shortImage (IMG_ROWS, IMG_COLS, CV_16U, &data[0]);
	showImage("Short Data Image", shortImage, screen);

	// write short matrix
	//save_matrix_to_pgm_file(shortImage, inputName + "_short.pgm");
	//save_matrix_to_short_file(shortImage, inputName + "_short.raw");

	// convert matrix to float
	cv::Mat floatImage;
	shortImage.convertTo(floatImage, CV_32F, 1.0 / USHRT_MAX);
	showImage("Float Data Image", floatImage, screen);

	// write float matrix
	//save_matrix_to_pgm_file(floatImage, inputName + "_float.pgm");
	//save_matrix_to_short_file(floatImage, inputName + "_float.raw");

	cv::Mat dftImage;
	lineDftPerfom(floatImage, dftImage);

	cv::Mat magnitude;
	format_matrix_to_magnitude(dftImage, magnitude);
	//save_matrix_to_format_file(magnitude, inputName + "_magnitude.log");

	// save file to f32 
	save_matrix_to_binary_file(magnitude, inputName + "_magnitude.mag.original");
	//save_matrix_to_binary_file(magnitude, inputName + "_magnitude.raw.filtered");
	//save_matrix_to_short_file(magnitude, inputName + "_transform.raw");
	//print_matrix_to_log_file(magnitude, inputName + "_transform.log");

	//showDFT("DFT complex Image", dftImage, screen, false);
	//showDFT("DFT centered Image", dftImage, screen, true);

	//int chan = dftImage.channels();	// 2
	//int type = dftImage.type();		// 13 - CV_32FC2

	//////cv::Mat absolute;
	//////lineDftAbsolute(dftImage, absolute);
	//////lineDftFilePrint(absolute, "C:/Temp/LineWiseDftAbsolute.log");

	//cv::Mat filtered, filterview;
	//lineDftFilter(dftImage, filtered);
	//format_matrix_to_magnitude(filtered, filterview);
	//save_matrix_to_format_file(filterview, inputName + "_filtered.log");
	////lineDftFilePrint(filtered, "C:/Temp/LineWiseDftFiltered.log");

	//////cv::Size filterSize = cv::Size(4320, 4320);
	//////cv::Mat filterTest = createGaussianHighPassFilter(filterSize, 0);
	////

	////// apply filter
	//////shiftDFT(dft_row);
	//////cv::mulSpectrums(dft_row, filter, dft_row, DFT_ROWS, true);
	//////shiftDFT(dft_row);

	//cv::Mat inverted;
	//lineDftInvert(filtered, inverted);
	//showImage("DFT inverted float", inverted, screen);

	////chan = inverted.channels();		// 1
	////type = inverted.type();			// 5 - CV_32FC1

	////save_matrix_to_short_file(inverted, "c:/Temp/LineWiseInverted.raw");

	//save_matrix_to_pgm_file(inverted, "c:/Temp/LineWiseInverted.pgm");

	//cv::waitKey();

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
