#include "Response.h"

using namespace Testlab;

Response::Response()
{

}

Response::~Response()
{

}

Response& Response::operator=(const Response& another)
{
	// Data
	keys = another.keys;
	realValues = another.realValues;
	imagValues = another.imagValues;

	// Info
	type = another.type;
	path = another.path;
	originalRun = another.originalRun;
	name = another.name;
	node = another.node;
	component = another.component;
	direction = another.direction;
	dimension = another.dimension;
	channel = another.channel;
	numAverages = another.numAverages;
	sign = another.sign;
	transducer = another.transducer;

	return *this;
}
