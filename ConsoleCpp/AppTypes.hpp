#pragma once

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

