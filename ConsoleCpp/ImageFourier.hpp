#pragma once

#include <vector>
#include <cmath>
#include <algorithm>
#include <cstddef>
#include <cstdint>

class ImageFourier
{

private:

	const double M_PI = 3.14159265358979323846;


public:

	/*
	* Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
	* The vector can have any length. This is a wrapper function.
	*/
	void transform(std::vector<double> &real, std::vector<double> &imag);


	/*
	* Computes the inverse discrete Fourier transform (IDFT) of the given complex vector, storing the result back into the vector.
	* The vector can have any length. This is a wrapper function. This transform does not perform scaling, so the inverse is not a true inverse.
	*/
	void inverseTransform(std::vector<double> &real, std::vector<double> &imag);


	/*
	* Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
	* The vector's length must be a power of 2. Uses the Cooley-Tukey decimation-in-time radix-2 algorithm.
	*/
	void transformRadix2(std::vector<double> &real, std::vector<double> &imag);


	/*
	* Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
	* The vector can have any length. This requires the convolution function, which in turn requires the radix-2 FFT function.
	* Uses Bluestein's chirp z-transform algorithm.
	*/
	void transformBluestein(std::vector<double> &real, std::vector<double> &imag);


	/*
	* Computes the circular convolution of the given real vectors. Each vector's length must be the same.
	*/
	void convolve(const std::vector<double> &x, const std::vector<double> &y, std::vector<double> &out);


	/*
	* Computes the circular convolution of the given complex vectors. Each vector's length must be the same.
	*/
	void convolve(
		const std::vector<double> &xreal, const std::vector<double> &ximag,
		const std::vector<double> &yreal, const std::vector<double> &yimag,
		std::vector<double> &outreal, std::vector<double> &outimag);
};

// Private function prototypes
static size_t reverseBits(size_t x, int n);

void ImageFourier::transform(std::vector<double> &real, std::vector<double> &imag) 
{
	size_t n = real.size();
	if (n != imag.size())
		throw "Mismatched lengths";
	if (n == 0)
		return;
	else if ((n & (n - 1)) == 0)  // Is power of 2
		transformRadix2(real, imag);
	else  // More complicated algorithm for arbitrary sizes
		transformBluestein(real, imag);
}


void ImageFourier::inverseTransform(std::vector<double> &real, std::vector<double> &imag) {
	transform(imag, real);
}


void ImageFourier::transformRadix2(std::vector<double> &real, std::vector<double> &imag) {
	// Length variables
	size_t n = real.size();
	if (n != imag.size())
		throw "Mismatched lengths";
	int levels = 0;  // Compute levels = floor(log2(n))
	for (size_t temp = n; temp > 1U; temp >>= 1)
		levels++;
	if (static_cast<size_t>(1U) << levels != n)
		throw "Length is not a power of 2";

	// Trignometric tables
	std::vector<double> cosTable(n / 2);
	std::vector<double> sinTable(n / 2);
	for (size_t i = 0; i < n / 2; i++) {
		cosTable[i] = std::cos(2 * M_PI * i / n);
		sinTable[i] = std::sin(2 * M_PI * i / n);
	}

	// Bit-reversed addressing permutation
	for (size_t i = 0; i < n; i++) {
		size_t j = reverseBits(i, levels);
		if (j > i) {
			std::swap(real[i], real[j]);
			std::swap(imag[i], imag[j]);
		}
	}

	// Cooley-Tukey decimation-in-time radix-2 FFT
	for (size_t size = 2; size <= n; size *= 2) {
		size_t halfsize = size / 2;
		size_t tablestep = n / size;
		for (size_t i = 0; i < n; i += size) {
			for (size_t j = i, k = 0; j < i + halfsize; j++, k += tablestep) {
				size_t l = j + halfsize;
				double tpre = real[l] * cosTable[k] + imag[l] * sinTable[k];
				double tpim = -real[l] * sinTable[k] + imag[l] * cosTable[k];
				real[l] = real[j] - tpre;
				imag[l] = imag[j] - tpim;
				real[j] += tpre;
				imag[j] += tpim;
			}
		}
		if (size == n)  // Prevent overflow in 'size *= 2'
			break;
	}
}


void ImageFourier::transformBluestein(std::vector<double> &real, std::vector<double> &imag) {
	// Find a power-of-2 convolution length m such that m >= n * 2 + 1
	size_t n = real.size();
	if (n != imag.size())
		throw "Mismatched lengths";
	size_t m = 1;
	while (m / 2 <= n) {
		if (m > SIZE_MAX / 2)
			throw "Vector too large";
		m *= 2;
	}

	// Trignometric tables
	std::vector<double> cosTable(n), sinTable(n);
	for (size_t i = 0; i < n; i++) {
		unsigned long long temp = static_cast<unsigned long long>(i) * i;
		temp %= static_cast<unsigned long long>(n) * 2;
		double angle = M_PI * temp / n;
		// Less accurate alternative if long long is unavailable: double angle = M_PI * i * i / n;
		cosTable[i] = std::cos(angle);
		sinTable[i] = std::sin(angle);
	}

	// Temporary vectors and preprocessing
	std::vector<double> areal(m), aimag(m);
	for (size_t i = 0; i < n; i++) {
		areal[i] = real[i] * cosTable[i] + imag[i] * sinTable[i];
		aimag[i] = -real[i] * sinTable[i] + imag[i] * cosTable[i];
	}
	std::vector<double> breal(m), bimag(m);
	breal[0] = cosTable[0];
	bimag[0] = sinTable[0];
	for (size_t i = 1; i < n; i++) {
		breal[i] = breal[m - i] = cosTable[i];
		bimag[i] = bimag[m - i] = sinTable[i];
	}

	// Convolution
	std::vector<double> creal(m), cimag(m);
	convolve(areal, aimag, breal, bimag, creal, cimag);

	// Postprocessing
	for (size_t i = 0; i < n; i++) {
		real[i] = creal[i] * cosTable[i] + cimag[i] * sinTable[i];
		imag[i] = -creal[i] * sinTable[i] + cimag[i] * cosTable[i];
	}
}


void ImageFourier::convolve(const std::vector<double> &x, const std::vector<double> &y, std::vector<double> &out) {
	size_t n = x.size();
	if (n != y.size() || n != out.size())
		throw "Mismatched lengths";
	std::vector<double> outimag(n);
	convolve(x, std::vector<double>(n), y, std::vector<double>(n), out, outimag);
}


void ImageFourier::convolve(
	const std::vector<double> &xreal, const std::vector<double> &ximag,
	const std::vector<double> &yreal, const std::vector<double> &yimag,
	std::vector<double> &outreal, std::vector<double> &outimag) {

	size_t n = xreal.size();
	if (n != ximag.size() || n != yreal.size() || n != yimag.size()
		|| n != outreal.size() || n != outimag.size())
		throw "Mismatched lengths";

	std::vector<double> xr = xreal;
	std::vector<double> xi = ximag;
	std::vector<double> yr = yreal;
	std::vector<double> yi = yimag;
	transform(xr, xi);
	transform(yr, yi);

	for (size_t i = 0; i < n; i++) {
		double temp = xr[i] * yr[i] - xi[i] * yi[i];
		xi[i] = xi[i] * yr[i] + xr[i] * yi[i];
		xr[i] = temp;
	}
	inverseTransform(xr, xi);

	for (size_t i = 0; i < n; i++) {  // Scaling (because this FFT implementation omits it)
		outreal[i] = xr[i] / n;
		outimag[i] = xi[i] / n;
	}
}


static std::size_t reverseBits(std::size_t x, int n) {
	std::size_t result = 0;
	for (int i = 0; i < n; i++, x >>= 1)
		result = (result << 1) | (x & 1U);
	return result;
}
