#pragma once

#include <algorithm>
#include <numeric>
#include <complex>
#include <vector>
#include <iterator>
#include <iostream>
#include <valarray>
#include <cmath>
#include <map>

using Pixel16 = uint16_t;
using Pixel32 = uint32_t;
using Pixel64 = uint64_t;
using PixelF32 = float_t;
using PixelF64 = double_t;
template<class PixelType> using PixelCollection = std::vector<PixelType>;

template<typename Type>
using Complex = std::complex<Type>;

using Complex32 = std::complex<float>;
using Complex64 = std::complex<double>;
template<class ComplexType> using ComplexCollection = std::vector<ComplexType>;

using ComplexImage = ComplexCollection<Complex32>;
using ComplexMatrix = std::map<std::size_t, ComplexImage>;


template<typename Type>
Type saturate_cast(Type val) {
	return std::min(std::max(val, std::numeric_limits<Type>::min()), std::numeric_limits<Type>::max());
}

template<typename Type>
class SaturateTo {
public:
	constexpr Type operator()(const Type value, const Type alpha = 1/*, const Type beta = 0*/) const noexcept { 
		return saturate_cast<Type>(value); 
	}
};

template <typename F, typename ...Args>
auto timer(F f, std::string const &label, Args && ...args) 
{
	using namespace std::chrono;

	auto start = high_resolution_clock::now();
	auto holder = f(std::forward<Args>(args)...);
	auto stop = high_resolution_clock::now();
	std::cout << label << " time: " << duration_cast<nanoseconds>(stop - start).count() << "\n";

	return holder;
}

template <typename Type>
auto TransformFourier(const Type& s, bool back = false)
{
	ComplexImage result(s.begin(), s.end());

	const float PI = 3.14159265358979323846;
	const float pol{ 2.0f * PI * (back ? -1.0f : 1.0f) };
	const float div{ back ? 1.0f : float(s.size()) };

	auto sum_up([=, &s](std::size_t j) {
		return[=, &s](Complex32 c, std::size_t k) {
			return c + s[k] * std::polar<float>(1.0f, pol * k * j / float(s.size()));
		};
	});

	auto to_ft([=, &s](std::size_t j)
	{
		return std::accumulate(
			num_iterator{ 0 },
			num_iterator(s.size()),
			Complex32{},
			sum_up(j))
			/ div;
	});

	std::transform(num_iterator{ 0 }, num_iterator{ s.size() }, std::begin(result), to_ft);

	return result;
}
