#pragma once

#include <algorithm>
#include <numeric>
#include <complex>
#include <vector>
#include <iterator>
#include <iostream>
#include <valarray>
#include <cmath>

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

class num_iterator 
{
public:
	explicit num_iterator(std::size_t position) : i(position) {}
	std::size_t operator*() const { return i; }
	num_iterator& operator++() {
		++i;
		return *this;
	}
	bool operator!=(const num_iterator& other) const {
		return i != other.i;
	}
	bool operator==(const num_iterator& other) const {
		return !(*this != other);
	}
private:
	std::size_t i;

};

namespace std {
	template <>
	struct iterator_traits<num_iterator> {
		using iterator_category = std::forward_iterator_tag;
		using value_type = std::size_t;
	};
}