check-roslyn:
	@$(MAKE) validate-roslyn RESET_VERSIONS=1
	cd $(ROSLYN_PATH); \
	./mono-testing.sh "$(XUNIT)" || exit; \
	echo "done"
