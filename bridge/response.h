#pragma once

#include <vector>
#include <string>

#include "common.h"
#include "macros.h"

namespace Testlab
{
	struct BRIDGE_EXPORTS Response : public IResponse
	{
		Response();
		virtual ~Response();

		Response& operator=(const Response& another);
	};
}
