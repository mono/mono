test-local: copy-targets

copy-targets:
	for p in net_2_0 net_3_5 net_4_0; do \
		cp $(XBUILD_DIR)/xbuild/Microsoft.CSharp.targets $(topdir)/class/lib/$$p; \
		cp $(XBUILD_DIR)/xbuild/Microsoft.VisualBasic.targets $(topdir)/class/lib/$$p; \
		cp $(XBUILD_DIR)/xbuild/Microsoft.Silverlight*.targets $(topdir)/class/lib/$$p; \
	done
	cp $(XBUILD_DIR)/xbuild/2.0/Microsoft.Common.* $(topdir)/class/lib/net_2_0
	cp $(XBUILD_DIR)/xbuild/3.5/Microsoft.Common.* $(topdir)/class/lib/net_3_5
	cp $(XBUILD_DIR)/xbuild/4.0/Microsoft.Common.* $(topdir)/class/lib/net_4_0

clean-local: clean-target-files

clean-target-files:
	for p in net_2_0 net_3_5 net_4_0; do \
		rm -f $(topdir)/class/lib/$$p/Microsoft.Common.targets; \
		rm -f $(topdir)/class/lib/$$p/Microsoft.CSharp.targets; \
		rm -f $(topdir)/class/lib/$$p/Microsoft.VisualBasic.targets; \
		rm -f $(topdir)/class/lib/$$p/Microsoft.Silverlight*.targets; \
		rm -f $(topdir)/class/lib/$$p/Microsoft.Common.tasks; \
	done
