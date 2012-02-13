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

		asms="`$PKG_CONFIG --variable=Libraries mono.ssdp` `$PKG_CONFIG --variable=Libraries mono.upnp` `$PKG_CONFIG --variable=Libraries mono.upnp.dcp.mediaserver1`"
		for asm in $asms; do
			FILENAME=`basename $asm`
			if [[ "`echo $SEENBEFORE | grep $FILENAME`" = "" ]]; then
				MONOUPNP_ASSEMBLIES="$MONOUPNP_ASSEMBLIES $asm"
				[[ -r "$asm.config" ]] && MONOUPNP_ASSEMBLIES="$MONOUPNP_ASSEMBLIES $asm.config"
				[[ -r "$asm.mdb" ]] && MONOUPNP_ASSEMBLIES="$MONOUPNP_ASSEMBLIES $asm.mdb"
				SEENBEFORE="$SEENBEFORE $FILENAME"
			fi
		done
		AC_SUBST(MONOUPNP_ASSEMBLIES)

		AM_CONDITIONAL(UPNP_ENABLED, true)
	else
		AM_CONDITIONAL(UPNP_ENABLED, false)
	fi

])

