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
	name = another.name;
	path = another.path;
	originalRun = another.originalRun;
	node = another.node;
	direction = another.direction;

	return *this;
}
