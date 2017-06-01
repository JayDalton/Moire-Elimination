#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
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

int main(int argc, char ** argv)
{
	help(argv[0]);

	const char* filename = argc >= 2 ? argv[1] : "Assets/0001.jpg";

	cv::Mat I = cv::imread(filename, cv::IMREAD_GRAYSCALE);
	if (I.empty()) { return -1; }

	cv::Mat padded;                            //expand input image to optimal size
	int m = cv::getOptimalDFTSize(I.rows);
	int n = cv::getOptimalDFTSize(I.cols); // on the border add zero values
	cv::copyMakeBorder(I, padded, 0, m - I.rows, 0, n - I.cols, cv::BORDER_CONSTANT, cv::Scalar::all(0));

	cv::Mat planes[] = { cv::Mat_<float>(padded), cv::Mat::zeros(padded.size(), CV_32F) };
	cv::Mat complexI;
	cv::merge(planes, 2, complexI);         // Add to the expanded another plane with zeros

	cv::dft(complexI, complexI);            // this way the result may fit in the source matrix

										// compute the magnitude and switch to logarithmic scale
										// => log(1 + sqrt(Re(DFT(I))^2 + Im(DFT(I))^2))
	cv::split(complexI, planes);                   // planes[0] = Re(DFT(I), planes[1] = Im(DFT(I))
	cv::magnitude(planes[0], planes[1], planes[0]);// planes[0] = magnitude
	cv::Mat magI = planes[0];

	magI += cv::Scalar::all(1);                    // switch to logarithmic scale
	cv::log(magI, magI);

	// crop the spectrum, if it has an odd number of rows or columns
	magI = magI(cv::Rect(0, 0, magI.cols & -2, magI.rows & -2));

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

	imshow("Input Image", I);    // Show the result
	imshow("spectrum magnitude", magI);
	cv::waitKey();

	return 0;
}

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
