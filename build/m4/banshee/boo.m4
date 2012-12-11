AC_DEFUN([BANSHEE_CHECK_BOO],
[
	BOO_REQUIRED=0.8.1

	AC_ARG_ENABLE([boo],
		[AC_HELP_STRING([--enable-boo], [Enable boo language support])],
		[],
		[enable_boo=no]
	)

	if test "x$enable_boo" = "xno"; then
		AM_CONDITIONAL(HAVE_BOO, false)
	else
		PKG_CHECK_MODULES(BOO, boo >= $BOO_REQUIRED)
		AC_SUBST(BOO_LIBS)
		AM_CONDITIONAL(HAVE_BOO, true)
	fi
])

