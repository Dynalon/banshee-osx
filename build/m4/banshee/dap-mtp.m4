AC_DEFUN([BANSHEE_CHECK_DAP_MTP],
[
	LIBMTP_REQUIRED=0.3.0

	AC_ARG_ENABLE(mtp, AC_HELP_STRING([--disable-mtp], [Disable MTP DAP support]), , enable_mtp="yes")
	
	enable_libmtp="${enable_mtp}"

	PKG_CHECK_MODULES(LIBMTP,
		libmtp >= $LIBMTP_REQUIRED,
		enable_libmtp="$enable_libmtp", enable_libmtp=no)

	if test "x$enable_mtp" = "xyes" -a "x$enable_libmtp" = "xno"; then
		AC_MSG_ERROR([libmtp was not found or is not up to date. Please install libmtp of at least version $LIBMTP_REQUIRED, or disable MTP support by passing --disable-mtp])
	fi

	if test "x$enable_libmtp" = "xyes"; then
		AC_CHECK_MEMBER([struct LIBMTP_track_struct.modificationdate],
				LIBMTP_HAS_MODDATE=yes,
				LIBMTP_HAS_MODDATE=no,
				[[#include <libmtp.h>]])
	fi

	AM_CONDITIONAL(ENABLE_MTP, test "x$enable_libmtp" = "xyes")
	AM_CONDITIONAL(LIBMTP_TRACK_STRUCT_HAS_MODDATE, [test "$LIBMTP_HAS_MODDATE" = "yes"])
	AC_CHECK_SIZEOF(time_t)
	AM_CONDITIONAL(LIBMTP_SIZEOF_TIME_T_64, [test "x$ac_cv_sizeof_time_t" = "x8"])
])

