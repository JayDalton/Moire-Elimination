#include <opencv2/core.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>
#include <iostream>

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

void eliminateMoire(cv::Mat& image)
{
	CV_DbgAssert(image.depth() == CV_32F);

	int chanels = image.channels();	// 2 - complex

	int nRows = image.rows;
	int nCols = image.cols/* * chanels*/;

	double* p;
	for (int i = 0; i < /*nRows*/1; i++)
	{
		p = image.ptr<double>(i);
		for (int j = 0; j < nCols; j++)
		{
			std::cout << p[j] << ", ";
			std::cout << std::endl;
		}
	}
}

void linewiseIterate(cv::Mat& image) 
{
	cv::Mat dftInput;
	image.convertTo(dftInput, CV_32F);
	cv::Mat one_row_in_frequency_domain;
	for (int i = 0; i < 1/*dftInput.rows*/; i++)
	{
		cv::Mat one_row = dftInput.row(i);
		cv::dft(one_row, one_row_in_frequency_domain, cv::DFT_COMPLEX_OUTPUT);

		double* p = one_row_in_frequency_domain.ptr<double>(0);

		for (size_t j = 0; j < one_row_in_frequency_domain.cols; j++)
		{
			std::cout << p[j] << ", ";
		}
		std::cout << std::endl;
	}
}

#include "opencv2/core.hpp"
#include "opencv2/imgproc.hpp"
#include "opencv2/imgcodecs.hpp"
#include "opencv2/highgui.hpp"

#include <iostream>

using namespace cv;
using namespace std;

int main(int argc, char ** argv)
{
	help(argv[0]);

	const char* filename = argc >= 2 ? argv[1] : "Assets/0003.jpg";

	Mat I = imread(filename, IMREAD_GRAYSCALE);
	if (I.empty())
		return -1;

	Mat padded;                            //expand input image to optimal size
	int m = getOptimalDFTSize(I.rows);
	int n = getOptimalDFTSize(I.cols); // on the border add zero values
	copyMakeBorder(I, padded, 0, m - I.rows, 0, n - I.cols, BORDER_CONSTANT, Scalar::all(0));

	Mat planes[] = { Mat_<float>(padded), Mat::zeros(padded.size(), CV_32F) };
	Mat complexI;
	merge(planes, 2, complexI);         // Add to the expanded another plane with zeros

	dft(complexI, complexI);            // this way the result may fit in the source matrix

										// compute the magnitude and switch to logarithmic scale
										// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
	split(complexI, planes);                   // planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))
	magnitude(planes[0], planes[1], planes[0]);// planes[0] = magnitude
	Mat magI = planes[0];

	eliminateMoire(magI);

	magI += Scalar::all(1);                    // switch to logarithmic scale
	eliminateMoire(magI);
	log(magI, magI);

	eliminateMoire(magI);

	// crop the spectrum, if it has an odd number of rows or columns
	magI = magI(Rect(0, 0, magI.cols & -2, magI.rows & -2));

	// rearrange the quadrants of Fourier image  so that the origin is at the image center
	int cx = magI.cols / 2;
	int cy = magI.rows / 2;

	Mat q0(magI, Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
	Mat q1(magI, Rect(cx, 0, cx, cy));  // Top-Right
	Mat q2(magI, Rect(0, cy, cx, cy));  // Bottom-Left
	Mat q3(magI, Rect(cx, cy, cx, cy)); // Bottom-Right

	Mat tmp;                           // swap quadrants (Top-Left with Bottom-Right)
	q0.copyTo(tmp);
	q3.copyTo(q0);
	tmp.copyTo(q3);

	q1.copyTo(tmp);                    // swap quadrant (Top-Right with Bottom-Left)
	q2.copyTo(q1);
	tmp.copyTo(q2);

	normalize(magI, magI, 0, 1, NORM_MINMAX); // Transform the matrix with float values into a
											  // viewable image form (float between values 0 and 1).

	imshow("Input Image", I);    // Show the result
	imshow("spectrum magnitude", magI);
	waitKey();

	return 0;
}
//int main(int argc, char ** argv)
//{
//	cv::Mat image = cv::imread("Assets/0003.jpg", CV_LOAD_IMAGE_GRAYSCALE);
//	cv::imshow("Original", image);
//	cv::imwrite("c:/Temp/original.jpg", image);
//	cv::waitKey();
//
//	cv::Mat dftInput;
//	image.convertTo(dftInput, CV_32F);
//
//	double t = (double)cv::getTickCount();
//
//	cv::Mat dftImage;
//	cv::dft(dftInput, dftImage, cv::DFT_COMPLEX_OUTPUT);
//
//	eliminateMoire(dftImage);
//
//	cv::Mat dftImageInverse;
//	cv::dft(dftImage, dftImageInverse, cv::DFT_INVERSE | cv::DFT_REAL_OUTPUT | cv::DFT_SCALE);
//
//	t = ((double)cv::getTickCount() - t) / cv::getTickFrequency();
//	std::cout << "Times passed in sec: " << t << std::endl;
//
//	cv::Mat convertedImage;
//	dftImageInverse.convertTo(convertedImage, CV_8U);
//	cv::imwrite("c:/Temp/result.jpg", convertedImage);
//	cv::imshow("Converted", convertedImage);
//	cv::waitKey();
//
//	return 0;
//}


//#include "opencv2/highgui/highgui.hpp"
//#include <iostream>
//
//using namespace std;
//using namespace cv;

//int main()
//{
//	// Read image from file
//	// Make sure that the image is in grayscale
//	Mat img = imread("lena.JPG", 0);
//
//	Mat planes[] = { Mat_<float>(img), Mat::zeros(img.size(), CV_32F) };
//	Mat complexI;    //Complex plane to contain the DFT coefficients {[0]-Real,[1]-Img}
//	merge(planes, 2, complexI);
//	dft(complexI, complexI);  // Applying DFT
//
//							  // Reconstructing original imae from the DFT coefficients
//	Mat invDFT, invDFTcvt;
//	idft(complexI, invDFT, DFT_SCALE | DFT_REAL_OUTPUT); // Applying IDFT
//	invDFT.convertTo(invDFTcvt, CV_8U);
//	imshow("Output", invDFTcvt);
//
//	//show the image
//	imshow("Original Image", img);
//
//	// Wait until user press some key
//	waitKey(0);
//	return 0;
//}

//#include "opencv2/highgui/highgui.hpp"
//#include <iostream>
//
//using namespace std;
//using namespace cv;
//
//int main()
//{
//	// Read image from file
//	// Make sure that the image is in grayscale
//	Mat img = imread("lena.JPG", 0);
//
//	Mat dftInput1, dftImage1, inverseDFT, inverseDFTconverted;
//	img.convertTo(dftInput1, CV_32F);
//	dft(dftInput1, dftImage1, DFT_COMPLEX_OUTPUT);    // Applying DFT
//
//													  // Reconstructing original imae from the DFT coefficients
//	idft(dftImage1, inverseDFT, DFT_SCALE | DFT_REAL_OUTPUT); // Applying IDFT
//	inverseDFT.convertTo(inverseDFTconverted, CV_8U);
//	imshow("Output", inverseDFTconverted);
//
//	//show the image
//	imshow("Original Image", img);
//
//	// Wait until user press some key
//	waitKey(0);
//	return 0;
//}


//int main(int argc, char ** argv)
//{
//	cv::Mat originalImage = cv::imread("Assets/0002.jpg", CV_LOAD_IMAGE_GRAYSCALE);
//	cv::imshow("Original", originalImage);
//	cv::imwrite("c:/Temp/original.jpg", originalImage);
//	cv::waitKey();
//
//	cv::Mat originalFloat;
//	originalImage.convertTo(originalFloat, CV_32F/*, 1.0 / 255.0*/);
//
//	//cv::Mat dftOfOriginal;
//	//takeDFT(originalFloat, dftOfOriginal);
//	////recenterDFT(dftOfOriginal);
//	//showDFT("Magnitude DFT", dftOfOriginal);
//
//	cv::Mat fourierImage;
//	cv::dft(originalFloat, fourierImage, cv::DFT_COMPLEX_OUTPUT);
//	
//	//eliminateMoire(dftOfOriginal.size(), dftOfOriginal, 256 / 2, 256 / 2, 40, 40);
//	//showDFT("Changed DFT", dftOfOriginal);
//
//	//cv::Mat invertedDFT;
//	//invertDFT(dftOfOriginal, invertedDFT);
//	//showDFT("Inverted DFT", invertedDFT);
//
//	cv::Mat inverseImage;
//	cv::dft(fourierImage, inverseImage, cv::DFT_INVERSE | cv::DFT_REAL_OUTPUT | cv::DFT_SCALE);
//
//	cv::Mat convertedImage;
//	inverseImage.convertTo(convertedImage, CV_8U);
//	cv::imwrite("c:/Temp/result.jpg", convertedImage);
//
//	//cv::Mat output;
//	//createGaussian(cv::Size(256, 256), output, 256 / 2, 256 / 2, 20, 20);
//	//cv::imshow("Gaussian", output);
//	//cv::waitKey();
//
//	return 0;
//}

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

//int main(int argc, char** argv)
//{
//	if (argc != 2)
//	{
//		cout << " Usage: display_image ImageToLoadAndDisplay" << endl;
//		return -1;
//	}
//	Mat image;
//	image = imread(argv[1], IMREAD_COLOR); // Read the file
//	if (image.empty()) // Check for invalid input
//	{
//		cout << "Could not open or find the image" << std::endl;
//		return -1;
//	}
//	namedWindow("Display window", WINDOW_AUTOSIZE); // Create a window for display.
//	imshow("Display window", image); // Show our image inside it.
//	waitKey(0); // Wait for a keystroke in the window
//	return 0;
//}
