#pragma once

#include <string>
#include <filesystem>

#include "response.h"

namespace fs = std::filesystem;

namespace Testlab
{
	class BRIDGE_EXPORTS Project
	{
	public:
		Project();
		Project(fs::path const& pathFile);
		~Project();

		bool isValid() const;

		// Section
		void createSection(std::wstring const& section, bool isSelect);
		bool isSectionExist(std::wstring const& section);

		// Folder
		void createFolder(std::wstring const& section, std::wstring const& folder);
		bool isFolderExist(std::wstring const& section, std::wstring const& folder);

		// Responses
		std::vector<Response> getSelectedResponses();

	private:
		class Impl;
		Impl* mpImpl;
	};
}

