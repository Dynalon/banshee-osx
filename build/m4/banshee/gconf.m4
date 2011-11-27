AC_DEFUN([BANSHEE_CHECK_GCONF],
[
	AC_PATH_PROG(GCONFTOOL, gconftool-2, no)

	# libgconf check needed because its -devel pkg should contain AM_GCONF_SOURCE_2 macro, see bgo#604416
	PKG_CHECK_MODULES(LIBGCONF, gconf-2.0)

	# needed so autoconf doesn't complain before checking the existence of libgconf2-devel above
	m4_pattern_allow([AM_GCONF_SOURCE_2])

	AM_GCONF_SOURCE_2
])
