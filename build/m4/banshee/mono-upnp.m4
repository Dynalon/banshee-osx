AC_DEFUN([BANSHEE_CHECK_MONO_UPNP],
[
	MONOUPNP_REQUIRED=0.1

	AC_ARG_ENABLE(upnp, AC_HELP_STRING([--disable-upnp], [Disable UPnP support]), , enable_upnp="yes")

	if test "x$enable_upnp" = "xyes"; then
		has_mono-upnp=no
		PKG_CHECK_MODULES(MONO_UPNP,
			mono.ssdp >= $MONOUPNP_REQUIRED
			mono.upnp >= $MONOUPNP_REQUIRED
			mono.upnp.dcp.mediaserver1 >= $MONOUPNP_REQUIRED)

		AC_SUBST(MONO_UPNP_LIBS)

		AM_CONDITIONAL(UPNP_ENABLED, true)
	else
		AM_CONDITIONAL(UPNP_ENABLED, false)
	fi

])

