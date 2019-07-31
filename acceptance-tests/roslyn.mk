check-roslyn:
	@echo "Runnning roslyn tests using mono from PATH:"
	mono --version
	@$(MAKE) validate-roslyn RESET_VERSIONS=1
	cd $(ROSLYN_PATH); \
	./build.sh --restore --build --test --mono || exit; \
	echo "done"
