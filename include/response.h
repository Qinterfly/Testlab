#pragma once

#include <vector>
#include <string>

#include "macros.h"

namespace Testlab
{
	enum BRIDGE_EXPORTS ResponseType
	{
		kUnknown,
		kDisp,
		kVeloc,
		kAccel,
		kForce
	};

	struct BRIDGE_EXPORTS Response
	{
		Response();
		~Response();

		Response& operator=(const Response& another);

		// Data
		std::vector<double> keys;
		std::vector<double> realValues;
		std::vector<double> imagValues;

		// Info
		ResponseType type;
		std::wstring name;
		std::wstring path;
		std::wstring originalRun;
		std::wstring node;
		std::wstring direction;
	};
}
