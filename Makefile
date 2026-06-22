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

.PHONY: all build build-only deploy clean rebuild package info help

all: build

build: $(BUILD_DLL) deploy
	@echo "Build and deployment complete."
	@echo "  DLL : $(BUILD_DLL)"
	@echo "  To  : $(PLUGIN_DIR)"

$(BUILD_DLL): $(CSPROJ) XIVClearSS.cs
	@echo "Building $(PROJECT_NAME)..."
	dotnet build -c $(CONFIGURATION)
	@test -f $(BUILD_DLL) || (echo "Build failed - DLL not found" && exit 1)

deploy: $(BUILD_DLL) $(PLUGIN_DIR)
	@echo "Deploying plugin files..."
	@cp $(BUILD_DLL)   $(PLUGIN_DLL)
	@cp $(JSON_NAME)   $(PLUGIN_JSON)
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

package: $(BUILD_DLL)
	@echo "Creating release package..."
	@mkdir -p dist
	@rm -f dist/$(PROJECT_NAME).zip
	@cd $(BUILD_DIR) && \
		zip -q ../../dist/$(PROJECT_NAME).zip $(DLL_NAME) && \
		cd ../.. && \
		zip -q dist/$(PROJECT_NAME).zip $(JSON_NAME)
	@if [ -f dist/$(PROJECT_NAME).zip ]; then \
		echo "Package created: dist/$(PROJECT_NAME).zip"; \
		ls -lh dist/$(PROJECT_NAME).zip; \
	else \
		echo "Package creation failed"; exit 1; \
	fi

info:
	@echo "Project : $(PROJECT_NAME)"
	@echo "Build   : $(BUILD_DLL)"
	@echo "Install : $(PLUGIN_DIR)"
	@if [ -f $(BUILD_DLL) ]; then ls -lh $(BUILD_DLL); else echo "Not built yet."; fi

help:
	@echo "Targets: build | build-only | deploy | clean | rebuild | package | info"
