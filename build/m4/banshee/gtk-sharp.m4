AC_DEFUN([BANSHEE_CHECK_GTK_SHARP],
[
	GTKSHARP3_REQUIRED=2.99
	GTKSHARP2_REQUIRED=2.12

	dnl First check for gtk-sharp-3.0
	PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-3.0 >= $GTKSHARP3_REQUIRED, have_gtk3=yes, have_gtk3=no)
	if test "x$have_gtk3" = "xyes"; then
		AC_SUBST(GTKSHARP_LIBS)

		PKG_CHECK_MODULES(GLIBSHARP, glib-sharp-3.0 >= $GTKSHARP3_REQUIRED)
		AC_SUBST(GLIBSHARP_LIBS)

        gtk_version=3
		AM_CONDITIONAL(HAVE_GTK3, true)

        HAVE_GLIBSHARP_2_12_7=yes
		AM_CONDITIONAL(HAVE_GLIBSHARP_2_12_7, true)

        gtksharp_with_a11y=yes
		AM_CONDITIONAL(ENABLE_ATK, true)
	else
		dnl Fall back to gtk-sharp-2.0

		PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-2.0 >= $GTKSHARP2_REQUIRED)
		AC_SUBST(GTKSHARP_LIBS)

		PKG_CHECK_MODULES(GLIBSHARP, glib-sharp-2.0 >= $GTKSHARP2_REQUIRED)
		AC_SUBST(GLIBSHARP_LIBS)

		PKG_CHECK_MODULES(GLIBSHARP_2_12_7,
			glib-sharp-2.0 >= 2.12.7,
			HAVE_GLIBSHARP_2_12_7=yes,
			HAVE_GLIBSHARP_2_12_7=no)
		AM_CONDITIONAL(HAVE_GLIBSHARP_2_12_7, [test "$HAVE_GLIBSHARP_2_12_7" = "yes"])

		PKG_CHECK_MODULES(GTKSHARP_A11Y, gtk-sharp-2.0 >= 2.12.10, gtksharp_with_a11y=yes, gtksharp_with_a11y=no)
		AM_CONDITIONAL(ENABLE_ATK, test "x$gtksharp_with_a11y" = "xyes")

		AM_CONDITIONAL(HAVE_GTK3, false)
        gtk_version="2"
	fi
])
