#pragma once

#include <string>
#include <filesystem>

#include "response.h"

namespace fs = std::filesystem;

namespace Testlab
{
	class BRIDGE_EXPORTS Project : public IProject
	{
	public:
		Project();
		Project(fs::path const& pathFile);
		virtual ~Project();

		bool isValid() const override;
		std::wstring path() const override;

		// Section
		void createSection(std::wstring const& section, bool isSelect) override;
		bool isSectionExist(std::wstring const& section) override;

		// Folder
		void createFolder(std::wstring const& section, std::wstring const& folder) override;
		bool isFolderExist(std::wstring const& section, std::wstring const& folder) override;

		// Responses
		std::vector<IResponse*> getResponses(std::vector<std::wstring> const& paths) override;
		std::vector<IResponse*> getSelectedResponses() override;
		bool addResponses(std::vector<IResponse*> const& responses, std::wstring const& path);

	private:
		class Impl;
		Impl* mpImpl;
	};
}

