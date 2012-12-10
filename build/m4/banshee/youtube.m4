AC_DEFUN([BANSHEE_CHECK_YOUTUBE],
[
	GDATASHARP_REQUIRED_VERSION=1.4

	AC_ARG_ENABLE(youtube, AC_HELP_STRING([--disable-youtube], [Disable Youtube extension]), , enable_youtube="yes")

	if test "x$enable_youtube" = "xyes"; then

		has_gdata=no
		PKG_CHECK_MODULES(GDATASHARP,
			gdata-sharp-youtube >= $GDATASHARP_REQUIRED_VERSION,
			has_gdata=yes, has_gdata=no)
		if test "x$has_gdata" = "xno"; then
			AC_MSG_ERROR([gdata-sharp-youtube was not found or is not up to date. Please install gdata-sharp-youtube of at least version $GDATASHARP_REQUIRED_VERSION, or disable YouTube extension by passing --disable-youtube])
		fi

		PKG_CHECK_MODULES(GDATASHARP_1_5_OR_HIGHER,
			gdata-sharp-youtube >= 1.5,
			[AM_CONDITIONAL(HAVE_GDATASHARP_1_5, true)],
			[AM_CONDITIONAL(HAVE_GDATASHARP_1_5, false)]
		)
		AC_SUBST(GDATASHARP_LIBS)
		AM_CONDITIONAL(ENABLE_YOUTUBE, true)
	else
		AM_CONDITIONAL(HAVE_GDATASHARP_1_5, false)
		AM_CONDITIONAL(ENABLE_YOUTUBE, false)
	fi
])
