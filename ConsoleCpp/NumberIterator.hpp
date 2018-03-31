#pragma once

#include <iterator>

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