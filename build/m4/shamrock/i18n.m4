AC_DEFUN([SHAMROCK_CONFIGURE_I18N],
[
	ALL_LINGUAS=`grep -v '^#' $srcdir/po/LINGUAS | $SED ':a;N;$!ba;s/\n/ /g; s/[ ]+/ /g' | xargs`
	GETTEXT_PACKAGE=$1
	AC_SUBST(GETTEXT_PACKAGE)
	AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, "$GETTEXT_PACKAGE", [Gettext Package])

	# needed so autoconf doesn't complain before checking the existence of glib-2.0 in configure.ac
	m4_pattern_allow([AM_GLIB_GNU_GETTEXT])
	AM_GLIB_GNU_GETTEXT

	AC_SUBST([CONFIG_STATUS_DEPENDENCIES],['$(top_srcdir)/po/LINGUAS'])
])

