check-roslyn:
	@$(MAKE) validate-roslyn RESET_VERSIONS=1
	cd $(ROSLYN_PATH); \
	./build.sh --restore --build --test --mono || exit; \
	echo "done"
