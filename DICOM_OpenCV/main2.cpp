#include <opencv2/core.hpp>
#include <opencv2/opencv.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>
#include <iostream>

using namespace cv;
using namespace std;

void onMouse(int event, int x, int y, int, void* param);
void updateMag(Mat complex);
void updateResult(Mat complex);

Mat computeDFT(Mat image);
Mat createGausFilterMask(Size mask_size, int x, int y, int ksize, bool normalization, bool invert);
void shift(Mat magI);

int kernel_size = 0;

int main2(int argc, char** argv)
{

	String file;
	file = "Assets/0001.png";

	Mat image = imread(file, CV_LOAD_IMAGE_GRAYSCALE);
	namedWindow("Orginal window", CV_WINDOW_AUTOSIZE);// Create a window for display.
	imshow("Orginal window", image);                   // Show our image inside it.

	Mat complex = computeDFT(image);

	namedWindow("spectrum", CV_WINDOW_AUTOSIZE);
	createTrackbar("Gausian kernel size", "spectrum", &kernel_size, 255, 0);
	setMouseCallback("spectrum", onMouse, &complex);

	updateMag(complex);         // compute magnitude of complex, switch to logarithmic scale and display...
	updateResult(complex);      // do inverse transform and display the result image
	waitKey(0);

	return 0;
}

void onMouse(int event, int x, int y, int, void* param)
{
	if (event != CV_EVENT_LBUTTONDOWN)
		return;
	// cast *param to use it local
	Mat* p_complex = (Mat*)param;
	Mat complex = *p_complex;

	Mat mask = createGausFilterMask(complex.size(), x, y, kernel_size, true, true);
	// show the kernel
	imshow("gaus-mask", mask);

	shift(mask);

	Mat planes[] = { Mat::zeros(complex.size(), CV_32F), Mat::zeros(complex.size(), CV_32F) };
	Mat kernel_spec;
	planes[0] = mask; // real
	planes[1] = mask; // imaginar
	merge(planes, 2, kernel_spec);

	mulSpectrums(complex, kernel_spec, complex, DFT_ROWS); // only DFT_ROWS accepted

	updateMag(complex);     // show spectrum
	updateResult(complex);      // do inverse transform

	*p_complex = complex;

	return;
}

void updateResult(Mat complex)
{
	Mat work;
	idft(complex, work);
	//  dft(complex, work, DFT_INVERSE + DFT_SCALE);
	Mat planes[] = { Mat::zeros(complex.size(), CV_32F), Mat::zeros(complex.size(), CV_32F) };
	split(work, planes);                // planes[0] = Re(DFT(I)), planes[1] = Im(DFT(I))

	magnitude(planes[0], planes[1], work);    // === sqrt(Re(DFT(I))^2 + Im(DFT(I))^2)
	normalize(work, work, 0, 1, NORM_MINMAX);
	imshow("result", work);
}

void updateMag(Mat complex)
{

	Mat magI;
	Mat planes[] = { Mat::zeros(complex.size(), CV_32F), Mat::zeros(complex.size(), CV_32F) };
	split(complex, planes);                // planes[0] = Re(DFT(I)), planes[1] = Im(DFT(I))

	magnitude(planes[0], planes[1], magI);    // sqrt(Re(DFT(I))^2 + Im(DFT(I))^2)

											  // switch to logarithmic scale: log(1 + magnitude)
	magI += Scalar::all(1);
	log(magI, magI);

	shift(magI);
	normalize(magI, magI, 1, 0, NORM_INF); // Transform the matrix with float values into a
										   // viewable image form (float between values 0 and 1).
	imshow("spectrum", magI);
}

//#include "dft_routines.h";

Mat computeDFT(Mat image) {
	// http://opencv.itseez.com/doc/tutorials/core/discrete_fourier_transform/discrete_fourier_transform.html
	Mat padded;                            //expand input image to optimal size
	int m = getOptimalDFTSize(image.rows);
	int n = getOptimalDFTSize(image.cols); // on the border add zero values
	copyMakeBorder(image, padded, 0, m - image.rows, 0, n - image.cols, BORDER_CONSTANT, Scalar::all(0));
	Mat planes[] = { Mat_<float>(padded), Mat::zeros(padded.size(), CV_32F) };
	Mat complex;
	merge(planes, 2, complex);         // Add to the expanded another plane with zeros
	dft(complex, complex, DFT_COMPLEX_OUTPUT);  // furier transform
	return complex;
}

Mat createGausFilterMask(Size mask_size, int x, int y, int ksize, bool normalization, bool invert) {
	// Some corrections if out of bounds
	if (x < (ksize / 2)) {
		ksize = x * 2;
	}
	if (y < (ksize / 2)) {
		ksize = y * 2;
	}
	if (mask_size.width - x < ksize / 2) {
		ksize = (mask_size.width - x) * 2;
	}
	if (mask_size.height - y < ksize / 2) {
		ksize = (mask_size.height - y) * 2;
	}

	// call openCV gaussian kernel generator
	double sigma = -1;
	Mat kernelX = getGaussianKernel(ksize, sigma, CV_32F);
	Mat kernelY = getGaussianKernel(ksize, sigma, CV_32F);
	// create 2d gaus
	Mat kernel = kernelX * kernelY.t();
	// create empty mask
	Mat mask = Mat::zeros(mask_size, CV_32F);
	Mat maski = Mat::zeros(mask_size, CV_32F);

	// copy kernel to mask on x,y
	Mat pos(mask, Rect(x - ksize / 2, y - ksize / 2, ksize, ksize));
	kernel.copyTo(pos);

	// create mirrored mask
	Mat posi(maski, Rect((mask_size.width - x) - ksize / 2, (mask_size.height - y) - ksize / 2, ksize, ksize));
	kernel.copyTo(posi);
	// add mirrored to mask
	add(mask, maski, mask);

	// transform mask to range 0..1
	if (normalization) {
		normalize(mask, mask, 0, 1, NORM_MINMAX);
	}

	// invert mask
	if (invert) {
		mask = Mat::ones(mask.size(), CV_32F) - mask;
	}

	return mask;
}

void shift(Mat magI) {

	// crop if it has an odd number of rows or columns
	magI = magI(Rect(0, 0, magI.cols & -2, magI.rows & -2));

	int cx = magI.cols / 2;
	int cy = magI.rows / 2;

	Mat q0(magI, Rect(0, 0, cx, cy));   // Top-Left - Create a ROI per quadrant
	Mat q1(magI, Rect(cx, 0, cx, cy));  // Top-Right
	Mat q2(magI, Rect(0, cy, cx, cy));  // Bottom-Left
	Mat q3(magI, Rect(cx, cy, cx, cy)); // Bottom-Right

	Mat tmp;                            // swap quadrants (Top-Left with Bottom-Right)
	q0.copyTo(tmp);
	q3.copyTo(q0);
	tmp.copyTo(q3);
	q1.copyTo(tmp);                     // swap quadrant (Top-Right with Bottom-Left)
	q2.copyTo(q1);
	tmp.copyTo(q2);
}