extension = {
filter_event : function(evt) {
	switch (evt.type) {
	case GF_JS_EVENT_PLAYBACK:
		this.set_option('SequenceIndex', ''+ evt.index);
		return false;
	default:
		return false;
	}
},

create_event_filter : function (__anobj) {
	return function (evt) {
		return __anobj.filter_event(evt);
	}
},

_event_filter : null,

start: function () {
	if (!this._event_filter) {
		this._event_filter = this.create_event_filter(this);
		gwlib_add_event_filter(this._event_filter);
	}
	var e = {};
	e.type = GF_JS_EVENT_PLAYLIST_RESET;
	gwlib_filter_event(e);

	e.type = GF_JS_EVENT_PLAYLIST_ADD;
	for (var i=0; i<this.sequences.length; i++) {
		e.url = this.sequences[i];
		gwlib_filter_event(e);
	}
	
	var sequence_index = parseInt( this.get_option('SequenceIndex', '0') );
	if (sequence_index >= this.sequences.length) sequence_index = 0;
	else if (sequence_index < 0) sequence_index = 0;
	
	var msg = gw_new_message(null, 'DASH-IF Playlist Loaded', '');
	msg.set_size(20 * gwskin.default_text_font_size, gwskin.default_icon_height + gwskin.default_text_font_size);
	msg.set_alpha(0.8);
	msg.show();

	e.type = GF_JS_EVENT_PLAYLIST_PLAY;
	e.index = sequence_index;
	gwlib_filter_event(e);
},

sequences : [
"https://dash.akamaized.net/dash264/TestCasesUHD/2b/11/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesUHD/2a/11/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/adapatationSetSwitching/5/manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/multiplePeriods/2/manifest_multiple_Periods_Add_Remove_AdaptationSet.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/multiplePeriods/1/manifest_multiple_Periods_Add_Remove_Representation.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/6/manifest_alternative_lang.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/4/manifest_alternative_Essentialproperty_live.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/1/manifest_alternative_content_live.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/3/manifest_alternative_maxWidth_live.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/2/manifest_alternative_content_ondemand.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/7/360_VR_BavarianAlpsWimbachklamm-AlternativeContent-with-ViewPoint.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/associative_content/1/manifest_associated_content_live.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/xHE-AAC_Stereo/2/Sintel/sintel_audio_video_brs.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/xHE-AAC_Stereo/1/Sintel/sintel_audio_only_brs.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/xHE-AAC_Stereo/3/Sintel/sintel_audio_only_64kbps.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_AVC_MultiRate_MultiRes_AAC_Eng_WebVTT.mpd",
"https://dash.akamaized.net/dash264/TestCases/2c/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCases/2c/qualcomm/2/MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2b/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2b/qualcomm/2/MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2b/DTV/1/live.mpd",
"https://dash.akamaized.net/dash264/TestCases/2b/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCases/2b/qualcomm/2/MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2c/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCases/2a/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCases/2a/qualcomm/2/MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2a/qualcomm/1/MultiResMPEG2.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/2a/qualcomm/2/MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCases/1b/qualcomm/1/MultiRatePatched.mpd",
"https://dash.akamaized.net/dash264/TestCases/1b/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/1c/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/1c/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/1b/qualcomm/1_1/MultiRatePatched.mpd",
"https://dash.akamaized.net/dash264/TestCases/1a/netflix/exMPD_BIP_TC1.mpd",
"https://dash.akamaized.net/dash264/TestCases/1a/netflix/exMPD_BIP_TC1.mpd",
"https://dash.akamaized.net/dash264/TestCases/1a/sony/SNE_DASH_SD_CASE1A_REVISED.mpd",
"https://dash.akamaized.net/dash264/TestCases/1a/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/1a/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_AVC_HEVC_MutliRate_MultiRes_IFrame_AAC.m3u8",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_Playready_AVC_HEVC_MultiRate_MultiRes_IFrame_AAC.m3u8",
"https://akamai-axtest.akamaized.net/routes/lapd-v1-acceptance/www_c4/Manifest.mpd",
"https://media.axprod.net/TestVectors/v8-MultiContent/Clear/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_AudioOnly_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_1080p_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_AudioOnly_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_1080p_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_AudioOnly_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_1080p_ClearKey.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/WSSReporting-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTPSReporting-Conf-OK-MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesNegative/2/1.mpd",
"https://dash.akamaized.net/dash264/TestCasesNegative/2/2.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Clear/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Clear/Manifest.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cbcs/Manifest.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cenc/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cenc/Manifest.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cbcs/Manifest_1080p.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/multiplePeriods/4/manifest_multiple_Periods_Different_SegmentDuration.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/6/DashIf-HDR10_UHD.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/6/DashIf-SDR_UHD.mpd",
"http://54.72.87.160/stattodyn/statodyn.php?type=mpd&pt=1376172180&tsbd=120&origmpd=https%3A%2F%2Fdash.akamaized.net%2Fdash264%2FTestCases%2F1b%2Fqualcomm%2F2%2FMultiRate.mpd&php=http%3A%2F%2Fdasher.eu5.org%2Fstatodyn.php&mpd=&debug=0&hack=.mpd",
"http://54.72.87.160/stattodyn/statodyn.php?type=mpd&pt=1376172390&tsbd=120&origmpd=http%3A%2F%2Fdash.akamaized.net%2Fdash264%2FTestCases%2F1b%2Fqualcomm%2F2%2FMultiRate.mpd&php=http%3A%2F%2Fdasher.eu5.org%2Fstatodyn.php&mpd=&debug=0&hack=.mpd",
"http://54.72.87.160/stattodyn/statodyn.php?type=mpd&pt=1376172485&tsbd=10&origmpd=http%3A%2F%2Fdash.akamaized.net%2Fdash264%2FTestCases%2F1b%2Fqualcomm%2F1%2FMultiRatePatched.mpd&php=http%3A%2F%2Fdasher.eu5.org%2Fstatodyn.php&mpd=&debug=0&hack=.mpd",
"https://dash.akamaized.net/dash264/TestCasesNegative/1/1.mpd",
"https://dash.akamaized.net/dash264/TestCasesNegative/1/2.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/MPDChaining/fallback_chain/1/manifest_fallback_MPDChaining.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/MPDChaining/fallback_chain/2/manifest_terminationEvent_fallback_MPDChaining.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/15/tos_live_multires_10bit_hev.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/17/bbb_live_multires_10bit_hev.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2a/12/tos_ondemand_multires_10bit_hev.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1a/5/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1b/5/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHDR/3b/3/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHDR/3a/3/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/16/tos_live_multires_10bit_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/18/bbb_live_multires_10bit_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2a/13/tos_ondemand_multires_10bit_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/14/tos_live_multires_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2a/11/tos_ondemand_multires_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1b/10/tos_live_multirate_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1a/9/tos_ondemand_multirate_hvc.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_HEVC_MultiRate_MultiRes_IFrame_AAC_WebVTT.m3u8",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_HEVC_MultiRate_MultiRes_AAC_Eng_TTML.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/CMAF/UnifiedStreaming/ToS_HEVC_MultiRate_MultiRes_AAC_Eng_WebVTT.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/1/TOS_Live_HEVC_MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2b/2/BBB_Live_HEVC_MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2a/1/TOS_OnDemand_HEVC_MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/2a/2/BBB_OnDemand_HEVC_MultiRes.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1b/1/TOS_Live_HEVC_MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1b/2/BBB_Live_HEVC_MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1a/1/TOS_OnDemand_HEVC_MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHEVC/1a/2/BBB_OnDemand_HEVC_MultiRate.mpd",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Clear/Manifest_1080p.m3u8",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Clear/Manifest.m3u8",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cbcs/Manifest_1080p.m3u8",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cbcs/Manifest.m3u8",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cenc/Manifest_1080p.m3u8",
"https://media.axprod.net/TestVectors/v9-MultiFormat/Encrypted_Cenc/Manifest.m3u8",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTPHeader-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTP-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTPS-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTPSQuery-OK-MultiRes.mpd",
"https://dash.akamaized.net/dash264/CTA/imsc1/IT1-20171027_dash.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/LastSegmentNumber/1/manifest_last_segment_num.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/xlink_30/insertad_2/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/xlink_30/insertad_4/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/start_1800/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/xlink_30/insertad_5/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/scte35_2/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/mup_300/tsbd_500/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/baseurl_d40_u20/baseurl_u40_d20/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/baseurl_u10_d20/baseurl_d10_u20/periods_10/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/periods_20/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/continuous_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/etp_30/etpDuration_10/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/etp_30/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_0/peroff_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_2/peroff_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/scte35_2/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/segtimeline_1/testpic_6s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/segtimeline_1/testpic_6s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/modulo_10/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/utc_direct-head/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/utc_ntp/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/utc_sntp/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim-dev/periods_60/xlink_30/insertad_3/testpic_2s/Manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesUHD/2b/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesUHD/2b/3/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesUHD/2b/4/MultiRate.mpd",
"https://akamai-axtest.akamaized.net/routes/lapd-v1-acceptance/www_c4/Manifest.m3u8",
"https://livesim.dashif.org/livesim-dev/periods_60/xlink_30/insertad_1/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/chunkdur_1/ato_7/testpic4_8s/Manifest300.mpd",
"https://livesim.dashif.org/livesim/chunkdur_1/ato_7/testpic4_8s/Manifest.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/HTTPSReporting-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/WSSReporting-OK-MultiRes.mpd",
"https://livesim.dashif.org/livesim/periods_60/mpdcallback_30/testpic_2s/Manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEGH_Stereo_lc_mha1/1/Sintel/sintel_audio_video_mpegh_mha1_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/1/1/ChID_voices_51_256_ddp.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/9/Living_Room_1080p_51_192k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/11/Living_Room_1080p_51_192k_320k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/10/Living_Room_1080p_51_192k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/12/Living_Room_1080p_51_192k_320k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/7/Living_Room_1080p_20_96k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/8/Living_Room_1080p_20_96k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/3/Living_Room_1080p_51_192k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/5/Living_Room_1080p_51_192k_320k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/4/Living_Room_1080p_51_192k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/6/Living_Room_1080p_51_192k_320k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/1/Living_Room_1080p_20_96k_25fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesDolby/2/Living_Room_1080p_20_96k_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/4/1/ChID_voices_71_384_448_768_ddp.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/2/1/ChID_voices_71_768_ddp.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dolby/3/1/ChID_voices_20_128_ddp.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/3/Paint_dtsc_testD.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/1/Paint_dtsc_testA.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/3/Paint_dtse_testD.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/1/Paint_dtse_testA.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/3/Paint_dtsh_testD.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/1/Paint_dtsh_testA.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/3/Paint_dtsl_testD.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/1/Paint_dtsl_testA.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/2/Paint_dtsc_testB.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/2/Paint_dtse_testB.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/dts/2/Paint_dtsl_testB.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/HE-AACv2_Multichannel/1/6chID/6chId_480p_single_adapt_heaac5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/HE-AACv2_Multichannel/2/8chID/8ch_id_480p_single_adapt_heaac7_1_cf12_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/HE-AACv2_Multichannel/3/ElephantsDream_6ch/elephants_dream_480p_heaac5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/HE-AACv2_Multichannel/3/Sintel_6ch/sintel_480p_heaac5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/HE-AACv2_Multichannel/3/Sintel_8ch/sintel_480p_heaac7_1_cf12_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEG_Surround/1/6chID/6chId_480p_single_adapt_mps5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEG_Surround/2/ElephantsDream_6ch/elephants_dream_480p_mps5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEG_Surround/2/Sintel_6ch/sintel_480p_mps5_1_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEGH_51_lc_mha1/1/Sintel/sintel_audio_video_mpegh_mha1_5_1_brs_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEGH_714_lc_mha1/1/Sintel/sintel_audio_video_mpegh_mha1_7_1_4_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesMCA/fraunhofer/MPEGH_Stereo_lc_mha1/1/Sintel/sintel_audio_video_mpegh_mha1_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1b.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1f.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1f.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1d.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1c.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1a.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1d.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1e.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/5_1e.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest_MultiPeriod_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest_MultiPeriod.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest_MultiPeriod_AudioOnly.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/sony/SNE_DASH_CASE3B_SD_REVISED.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/aac-lc_stereo_with_video/ElephantsDream/elephants_dream_480p_aaclc_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/aac-lc_stereo_with_video/Sintel/sintel_480p_aaclc_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/aac-lc_stereo_without_video/ElephantsDream/elephants_dream_audio_only_aaclc_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/aac-lc_stereo_without_video/Sintel/sintel_audio_only_aaclc_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/heaac_stereo_without_video/ElephantsDream/elephants_dream_audio_only_heaac_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/heaac_stereo_without_video/Sintel/sintel_audio_only_heaac_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/heaacv2_stereo_without_video/ElephantsDream/elephants_dream_audio_only_heaacv2_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3a/fraunhofer/heaacv2_stereo_without_video/Sintel/sintel_audio_only_heaacv2_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/heaac_stereo_with_video/ElephantsDream/elephants_dream_480p_heaac_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/heaac_stereo_with_video/Sintel/sintel_480p_heaac_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/heaacv2_stereo_with_video/ElephantsDream/elephants_dream_480p_heaacv2_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCases/3b/fraunhofer/heaacv2_stereo_with_video/Sintel/sintel_480p_heaacv2_stereo_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/multiplePeriods/3/manifest_multiple_Periods_Content_Offering_CDN.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1b.mpd",
"https://dash.akamaized.net/dash264/TestCases/5a/nomor/1.mpd",
"https://dash.akamaized.net/dash264/TestCases/5a/nomor/3.mpd",
"https://dash.akamaized.net/dash264/TestCases/5a/nomor/5.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/3.mpd",
"https://dash.akamaized.net/dash264/TestCases/5a/nomor/4.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/1.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/2.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/6.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/7.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/10.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/11.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/4.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/5.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/8.mpd",
"https://dash.akamaized.net/dash264/TestCases/5b/nomor/9.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1c.mpd",
"https://dash.akamaized.net/dash264/TestCases/5c/nomor/4_1a.mpd",
"https://dash.akamaized.net/dash264/TestCases/4b/qualcomm/2/TearsOfSteel_onDem5secSegSubTitles.mpd",
"https://dash.akamaized.net/dash264/TestCases/4b/qualcomm/1/ED_OnDemand_5SecSeg_Subtitles.mpd",
"https://dash.akamaized.net/dash264/TestCases/10a/1/iis_forest_short_poem_multi_lang_480p_single_adapt_aaclc_sidx.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/adapatationSetSwitching/4/manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/adapatationSetSwitching/2/manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesUHD/2a/5/MultiRate.mpd",
"https://media.axprod.net/TestVectors/v8-MultiContent/Encrypted/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_AudioOnly.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_AudioOnly.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_AudioOnly.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_AudioOnly.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_AudioOnly.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_AudioOnly.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/adapatationSetSwitching/3/manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/adapatationSetSwitching/1/manifest.mpd",
"https://livesim.dashif.org/livesim/testpic_2s/Manifest.mpd#t=posix:1465406946",
"https://livesim.dashif.org/livesim/testpic_2s/Manifest.mpd#t=posix:now",
"https://dash.akamaized.net/dash264/TestCasesIOP33/MPDChaining/regular_chain/1/manifest_regular_MPDChaining_live.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/MPDChaining/regular_chain/2/manifest_regular_MPDChaining_OnDemand.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP33/resolveToZero/1/manifest.mpd",
"https://dash.akamaized.net/dash264/TestCases/4b/qualcomm/3/Solekai.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest_1080p.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest.mpd",
"https://media.axprod.net/TestVectors/v7-Clear/Manifest_AudioOnly.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/1a/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/1a/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/1b/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/4c/1/dash.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/1b/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/1c/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/MultiPeriod_OnDemand/ThreePeriods/ThreePeriod_OnDemand2.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/MultiPeriod_OnDemand/ThreePeriods/ThreePeriod_OnDemand_presentationDur.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/MultiPeriod_OnDemand/ThreePeriods/ThreePeriod_OnDemand_presentationDur_AudioTrim.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_AVC_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_AVC_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_HEVC_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_HEVC_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_MultiCodec_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/CMS/Axinom-CMS_MultiCodec_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_AVC_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_AVC_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_HEVC_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_HEVC_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_MultiCodec_MultiRes_MultiRate_25fps.mpd",
"https://dash.akamaized.net/dash264/TestAdvertising/DRM/Axinom-DRM-in-disconnected-environments_MultiCodec_MultiRes_MultiRate_2997fps.mpd",
"https://dash.akamaized.net/dash264/TestCases/9b/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/9b/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/9c/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/9a/qualcomm/1/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCases/9a/qualcomm/2/MultiRate.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/MultiPeriod_OnDemand/TwoPeriods/TwoPeriod_OnDemand.mpd",
"https://dash.akamaized.net/dash264/TestCasesHD/MultiPeriod_OnDemand/TwoPeriods/TwoPeriod_OnDemand_presentationDur.mpd",
"https://dash.akamaized.net/dash264/TestCasesIOP41/MultiTrack/alternative_content/5/manifest_alternative_ToS_Viewpoint.mpd",
"https://livesim.dashif.org/livesim/utc_direct/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/utc_head/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/utc_ntp/testpic_2s/Manifest.mpd",
"https://livesim.dashif.org/livesim/utc_sntp/testpic_2s/Manifest.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-hd-adaptive/sintel-vp9-hd-adaptive.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-hd/sintel-vp9-hd.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-hd-hdr/sintel-vp9-hd-hdr.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-hd-encrypted/sintel-vp9-hd-encrypted.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-uhd/sintel-vp9-uhd.mpd",
"https://dash.akamaized.net/dash264/TestCasesVP9/vp9-uhd-hdr/sintel-vp9-uhd-hdr.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_1080p_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey/Manifest_AudioOnly_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_1080p_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-MultiKey-MultiPeriod/Manifest_AudioOnly_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_1080p_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_ClearKey.mpd",
"https://media.axprod.net/TestVectors/v7-MultiDRM-SingleKey/Manifest_AudioOnly_ClearKey.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/WS-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/WSS-OK-MultiRes.mpd",
"https://raw.githubusercontent.com/Dash-Industry-Forum/SAND-Test-Vectors/master/mpd/dash-if/WSSQuery-OK-MultiRes.mpd"
]

};
