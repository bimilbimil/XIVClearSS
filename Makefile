PROJECT_NAME = XIVClearSS
CSPROJ       = $(PROJECT_NAME).csproj
DLL_NAME     = $(PROJECT_NAME).dll
JSON_NAME    = $(PROJECT_NAME).json

BUILD_DIR  = bin/Debug
BUILD_DLL  = $(BUILD_DIR)/$(DLL_NAME)
BUILD_JSON = $(BUILD_DIR)/$(JSON_NAME)

PLUGIN_DIR  = ~/Library/Application\ Support/XIV\ on\ Mac/dalamud/Hooks/dev/plugins
PLUGIN_DLL  = $(PLUGIN_DIR)/$(DLL_NAME)
PLUGIN_JSON = $(PLUGIN_DIR)/$(JSON_NAME)

CONFIGURATION = Debug

.PHONY: all build build-only deploy clean rebuild package package-dev info help

all: build

# Build and deploy to local dev plugin directory
build: $(BUILD_DLL) deploy
	@echo "Build and deployment complete."
	@echo "  DLL : $(BUILD_DLL)"
	@echo "  To  : $(PLUGIN_DIR)"

$(BUILD_DLL): $(CSPROJ) $(PROJECT_NAME).cs
	@echo "Building $(PROJECT_NAME)..."
	dotnet build -c $(CONFIGURATION)
	@test -f $(BUILD_DLL) || (echo "Build failed - DLL not found" && exit 1)

deploy: $(BUILD_DLL) $(PLUGIN_DIR)
	@echo "Deploying plugin files..."
	@cp $(BUILD_DLL) $(PLUGIN_DLL)
	@cp $(JSON_NAME) $(PLUGIN_JSON)
	@echo "  Copied $(DLL_NAME)"
	@echo "  Copied $(JSON_NAME)"

$(PLUGIN_DIR):
	@mkdir -p $(PLUGIN_DIR)

build-only:
	dotnet build -c $(CONFIGURATION)

clean:
	dotnet clean
	@echo "Clean complete."

rebuild: clean build

# Create a release zip and update repo.json
# Usage: make package [RELEASE_TAG=v1.0.0]
package: $(BUILD_DLL)
	@echo "Creating release package..."
	@echo "Updating repo.json LastUpdate and AssemblyVersion..."
	@TIMESTAMP=$$(date +%s); \
	if command -v jq >/dev/null 2>&1; then \
		CURRENT_VERSION=$$(jq -r '.[0].AssemblyVersion' repo.json); \
		MAJOR=$$(echo $$CURRENT_VERSION | cut -d. -f1); \
		MINOR=$$(echo $$CURRENT_VERSION | cut -d. -f2); \
		PATCH=$$(echo $$CURRENT_VERSION | cut -d. -f3); \
		BUILD=$$(echo $$CURRENT_VERSION | cut -d. -f4); \
		NEW_BUILD=$$((BUILD + 1)); \
		NEW_VERSION="$$MAJOR.$$MINOR.$$PATCH.$$NEW_BUILD"; \
		REPO_URL=$$(jq -r '.[0].RepoUrl' repo.json); \
		if [ -n "$$RELEASE_TAG" ]; then \
			DOWNLOAD_URL="$$REPO_URL/releases/download/$$RELEASE_TAG/$(PROJECT_NAME).zip"; \
			jq --arg ts $$TIMESTAMP --arg ver $$NEW_VERSION --arg url $$DOWNLOAD_URL \
				'.[0].LastUpdate = ($$ts | tonumber) | .[0].AssemblyVersion = $$ver | .[0].DownloadLinkInstall = $$url | .[0].DownloadLinkUpdate = $$url | .[0].DownloadLinkTesting = $$url' \
				repo.json > repo.json.tmp && mv repo.json.tmp repo.json; \
			echo "  Updated LastUpdate, AssemblyVersion to $$NEW_VERSION, DownloadLinks to $$RELEASE_TAG"; \
		else \
			jq --arg ts $$TIMESTAMP --arg ver $$NEW_VERSION \
				'.[0].LastUpdate = ($$ts | tonumber) | .[0].AssemblyVersion = $$ver' \
				repo.json > repo.json.tmp && mv repo.json.tmp repo.json; \
			echo "  Updated LastUpdate and AssemblyVersion to $$NEW_VERSION"; \
			echo "  Note: DownloadLinks unchanged. Run: make package RELEASE_TAG=v1.0.1 to update them."; \
		fi; \
		jq --arg ver $$NEW_VERSION '.AssemblyVersion = $$ver' $(JSON_NAME) > $(JSON_NAME).tmp && mv $(JSON_NAME).tmp $(JSON_NAME); \
		sed -i.bak "s/\"AssemblyVersion\": \"[^\"]*\"/\"AssemblyVersion\": \"$$NEW_VERSION\"/" $(PROJECT_NAME).yaml && rm -f $(PROJECT_NAME).yaml.bak; \
		echo "  Synced AssemblyVersion in $(JSON_NAME) and $(PROJECT_NAME).yaml"; \
	else \
		sed -i.bak "s/\"LastUpdate\": [0-9]*/\"LastUpdate\": $$TIMESTAMP/" repo.json && rm -f repo.json.bak; \
		echo "  Updated LastUpdate (jq not found — AssemblyVersion not bumped)"; \
	fi
	@mkdir -p dist
	@rm -f dist/$(PROJECT_NAME).zip
	@cd $(BUILD_DIR) && \
		([ -f $(DLL_NAME) ] && zip -q ../../dist/$(PROJECT_NAME).zip $(DLL_NAME) || (echo "DLL not found" && exit 1)) && \
		([ -f $(DLL_NAME:.dll=.deps.json) ] && zip -q ../../dist/$(PROJECT_NAME).zip $(DLL_NAME:.dll=.deps.json) || true) && \
		cd ../.. && \
		zip -q dist/$(PROJECT_NAME).zip $(JSON_NAME) && \
		([ -f $(PROJECT_NAME).yaml ] && zip -q dist/$(PROJECT_NAME).zip $(PROJECT_NAME).yaml || true)
	@if [ -f dist/$(PROJECT_NAME).zip ]; then \
		echo "Package created: dist/$(PROJECT_NAME).zip"; \
		ls -lh dist/$(PROJECT_NAME).zip; \
		echo ""; \
		echo "Contents:"; \
		unzip -l dist/$(PROJECT_NAME).zip | grep -E "\.(dll|json)$$"; \
	else \
		echo "Package creation failed"; exit 1; \
	fi

# Create a dev zip (no version bump)
package-dev: $(BUILD_DLL)
	@echo "Creating dev package..."
	@mkdir -p dist
	@rm -f dist/$(PROJECT_NAME)-dev.zip
	@cd $(BUILD_DIR) && \
		([ -f $(DLL_NAME) ] && zip -q ../../dist/$(PROJECT_NAME)-dev.zip $(DLL_NAME) || (echo "DLL not found" && exit 1)) && \
		([ -f $(DLL_NAME:.dll=.deps.json) ] && zip -q ../../dist/$(PROJECT_NAME)-dev.zip $(DLL_NAME:.dll=.deps.json) || true) && \
		cd ../.. && \
		zip -q dist/$(PROJECT_NAME)-dev.zip $(JSON_NAME) && \
		([ -f $(PROJECT_NAME).yaml ] && zip -q dist/$(PROJECT_NAME)-dev.zip $(PROJECT_NAME).yaml || true)
	@if [ -f dist/$(PROJECT_NAME)-dev.zip ]; then \
		echo "Dev package: dist/$(PROJECT_NAME)-dev.zip"; \
		ls -lh dist/$(PROJECT_NAME)-dev.zip; \
	else \
		echo "Package creation failed"; exit 1; \
	fi

info:
	@echo "Project : $(PROJECT_NAME)"
	@echo "Build   : $(BUILD_DLL)"
	@echo "Install : $(PLUGIN_DIR)"
	@if [ -f $(BUILD_DLL) ]; then ls -lh $(BUILD_DLL); else echo "Not built yet."; fi

help:
	@echo "Targets:"
	@echo "  make build                        Build and deploy to dev plugin folder"
	@echo "  make build-only                   Build without deploying"
	@echo "  make package                      Build release zip, bump version in repo.json"
	@echo "  make package RELEASE_TAG=v1.0.0   Also update download links in repo.json"
	@echo "  make package-dev                  Build zip without bumping version"
	@echo "  make clean                        Clean build artifacts"
	@echo "  make rebuild                      Clean then build"
	@echo "  make info                         Show build info"
