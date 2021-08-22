﻿namespace ServiceLayer
{
    /// <summary>
    /// Array values are detected. Empty them or fir Handle AV bypass
    /// </summary>
    internal class SharedGlobals
    {

        // Thanks Harley!
        // https://github.com/harleyQu1nn/AggressorScripts/blob/master/ProcessColor.cna
        public static readonly string[] AVs =
        {
            "tanium.exe", "360rp.exe", "360sd.exe", "360safe.exe", "360leakfixer.exe", "360rp.exe", "360safe.exe",
            "360sd.exe",
            "360tray.exe", "aawtray.exe", "acaas.exe", "acaegmgr.exe", "acais.exe", "aclntusr.exe", "alert.exe",
            "alertsvc.exe", "almon.exe", "alunotify.exe",
            "alupdate.exe", "alsvc.exe", "avengine.exe", "avgchsvx.exe", "avgcsrvx.exe", "avgidsagent.exe",
            "avgidsmonitor.exe", "avgidsui.exe", "avgidswatcher.exe",
            "avgnsx.exe", "avkproxy.exe", "avkservice.exe", "avktray.exe", "avkwctl.exe", "avp.exe", "avp.exe",
            "avpdtagt.exe", "acctmgr.exe", "ad-aware.exe",
            "ad-aware2007.exe", "addressexport.exe", "adminserver.exe", "administrator.exe", "aexagentuihost.exe",
            "aexnsagent.exe", "aexnsrcvsvc.exe", "alertsvc.exe",
            "alogserv.exe", "aluschedulersvc.exe", "anvir.exe", "appsvc32.exe", "atrshost.exe", "auth8021x.exe",
            "avastsvc.exe", "avastui.exe", "avconsol.exe", "avpm.exe",
            "avsynmgr.exe", "avtask.exe", "blackd.exe", "bwmeterconsvc.exe", "caantispyware.exe", "calogdump.exe",
            "cappactiveprotection.exe", "cappactiveprotection.exe",
            "cb.exe", "ccap.exe", "ccenter.exe", "cclaw.exe", "clps.exe", "clpsla.exe", "clpsls.exe", "cntaosmgr.exe",
            "cpntsrv.exe", "ctdataload.exe",
            "certificationmanagerservicent.exe", "clshield.exe", "clamtray.exe", "clamwin.exe", "console.exe",
            "cylanceui.exe", "dao_log.exe", "dlservice.exe",
            "dltray.exe", "dltray.exe", "drwagntd.exe", "drwagnui.exe", "drweb32w.exe", "drwebscd.exe", "drwebupw.exe",
            "drwinst.exe", "dsmain.exe", "dwhwizrd.exe",
            "defwatch.exe", "dolphincharge.exe", "ehttpsrv.exe", "emet_agent.exe", "emet_service.exe", "emlproui.exe",
            "emlproxy.exe", "emlibupdateagentnt.exe",
            "etconsole3.exe", "etcorrel.exe", "etloganalyzer.exe", "etreporter.exe", "etrssfeeds.exe", "euqmonitor.exe",
            "endpointsecurity.exe", "engineserver.exe",
            "entitymain.exe", "etscheduler.exe", "etwcontrolpanel.exe", "eventparser.exe", "fameh32.exe", "fcdblog.exe",
            "fch32.exe", "fpavserver.exe", "fprottray.exe",
            "fscuif.exe", "fshdll32.exe", "fsm32.exe", "fsma32.exe", "fsmb32.exe", "fwcfg.exe", "firesvc.exe",
            "firetray.exe", "firewallgui.exe", "forcefield.exe",
            "fortiproxy.exe", "fortitray.exe", "fortiwf.exe", "frameworkservice.exe", "freeproxy.exe",
            "gdfirewalltray.exe", "gdfwsvc.exe", "hwapi.exe", "isntsysmonitor.exe",
            "issvc.exe", "iswmgr.exe", "itmrtsvc.exe", "itmrt_supportdiagnostics.exe", "itmrt_trace.exe", "icepack.exe",
            "idsinst.exe", "inonmsrv.exe", "inort.exe",
            "inorpc.exe", "inotask.exe", "inoweb.exe", "isntsmtp.exe", "kabackreport.exe", "kanmcmain.exe", "kavfs.exe",
            "kavstart.exe", "klnagent.exe", "kmailmon.exe",
            "knupdatemain.exe", "kpfwsvc.exe", "kswebshield.exe", "kvmonxp.exe", "kvmonxp_2.exe", "kvsrvxp.exe",
            "kwsprod.exe", "kwatch.exe", "kavadapterexe.exe",
            "keypass.exe", "kvxp.exe", "luall.exe", "lwdmserver.exe", "lockapp.exe", "lockapphost.exe", "loggetor.exe",
            "mcshield.exe", "mcui32.exe", "msascui.exe",
            "managementagentnt.exe", "mcafeedatabackup.exe", "mcepoc.exe", "mcepocfg.exe", "mcnasvc.exe", "mcproxy.exe",
            "mcscript_inuse.exe", "mcwce.exe",
            "mcwcecfg.exe", "mcshield.exe", "mctray.exe", "mgntsvc.exe", "mpcmdrun.exe", "mpfagent.exe", "mpfsrv.exe",
            "msmpeng.exe", "nailgpip.exe", "navapsvc.exe",
            "navapw32.exe", "ncdaemon.exe", "nip.exe", "njeeves.exe", "nlclient.exe", "nmagent.exe", "nod32view.exe",
            "npfmsg.exe", "nprotect.exe", "nrmenctb.exe",
            "nsmdtr.exe", "ntrtscan.exe", "nvcoas.exe", "nvcsched.exe", "navshcom.exe", "navapsvc.exe", "navectrl.exe",
            "navelog.exe", "navesp.exe", "navw32.exe",
            "navwnt.exe", "nip.exe", "njeeves.exe", "npfmsg2.exe", "npfsvice.exe", "nsctop.exe", "nvcoas.exe",
            "nvcsched.exe", "nymse.exe", "olfsnt40.exe", "omslogmanager.exe",
            "onlinent.exe", "onlnsvc.exe", "ofcpfwsvc.exe", "pasystemtray.exe", "pavfnsvr.exe", "pavsrv51.exe",
            "pnmsrv.exe", "poproxy.exe", "poproxy.exe", "ppclean.exe",
            "ppctlpriv.exe", "pqibrowser.exe", "pshost.exe", "psimsvc.exe", "pxemtftp.exe", "padfsvr.exe", "pagent.exe",
            "pagentwd.exe", "pavbckpt.exe", "pavfnsvr.exe",
            "pavprsrv.exe", "pavprot.exe", "pavreport.exe", "pavkre.exe", "pcctlcom.exe", "pcscnsrv.exe",
            "pccntmon.exe", "pccntupd.exe", "ppppwallrun.exe", "printdevice.exe",
            "proutil.exe", "psctrls.exe", "psimsvc.exe", "pwdfilthelp.exe", "qoeloader.exe", "ravmond.exe", "ravxp.exe",
            "rnreport.exe", "rpcserv.exe", "rssensor.exe",
            "rtvscan.exe", "rapapp.exe", "rav.exe", "ravalert.exe", "ravmon.exe", "ravmond.exe", "ravservice.exe",
            "ravstub.exe", "ravtask.exe", "ravtray.exe", "ravupdate.exe",
            "ravxp.exe", "realmon.exe", "realmon.exe", "redirsvc.exe", "regmech.exe", "reportersvc.exe", "routernt.exe",
            "rtvscan.exe", "safeservice.exe", "saservice.exe",
            "savadminservice.exe", "savfmsesp.exe", "savmain.exe", "savscan.exe", "scanmsg.exe", "scanwscs.exe",
            "scfmanager.exe", "scfservice.exe", "scftray.exe",
            "sdtrayapp.exe", "sevinst.exe", "smex_activeupdate.exe", "smex_master.exe", "smex_remoteconf.exe",
            "smex_systemwatch.exe", "smsectrl.exe", "smselog.exe",
            "smsesjm.exe", "smsesp.exe", "smsesrv.exe", "smsetask.exe", "smseui.exe", "snac.exe", "snac.exe",
            "sndmon.exe", "sndsrvc.exe", "spbbcsvc.exe", "spiderml.exe",
            "spidernt.exe", "ssm.exe", "ssscheduler.exe", "svcharge.exe", "svdealer.exe", "svframe.exe", "svtray.exe",
            "swnetsup.exe", "savroam.exe", "savservice.exe",
            "savui.exe", "scanmailoutlook.exe", "seanalyzertool.exe", "semsvc.exe", "sesclu.exe", "setupguimngr.exe",
            "siteadv.exe", "smc.exe", "smcgui.exe", "snhwsrv.exe",
            "snicheckadm.exe", "snicon.exe", "snsrv.exe", "snichecksrv.exe", "spideragent.exe", "spntsvc.exe",
            "spyemergency.exe", "spyemergencysrv.exe", "stopp.exe",
            "stwatchdog.exe", "symcorpui.exe", "symsport.exe", "tbmon.exe", "tfgui.exe", "tfservice.exe", "tftray.exe",
            "tfun.exe", "tiaspn~1.exe", "tsansrf.exe", "tsatisy.exe",
            "tscutynt.exe", "tsmpnt.exe", "tmlisten.exe", "tmpfw.exe", "tmntsrv.exe", "traflnsp.exe",
            "traptrackermgr.exe", "upschd.exe", "ucservice.exe", "udaterui.exe",
            "umxagent.exe", "umxcfg.exe", "umxfwhlp.exe", "umxpol.exe", "up2date.exe", "updaterui.exe", "urllstck.exe",
            "useractivity.exe", "useranalysis.exe", "usrprmpt.exe",
            "v3medic.exe", "v3svc.exe", "vpc32.exe", "vpdn_lu.exe", "vptray.exe", "vsstat.exe", "vsstat.exe",
            "vstskmgr.exe", "webproxy.exe", "wfxctl32.exe", "wfxmod32.exe",
            "wfxsnt40.exe", "webproxy.exe", "webscanx.exe", "winroute.exe", "wrspysetup.exe", "zlh.exe", "zanda.exe",
            "zhudongfangyu.exe", "zlh.exe", "_avp32.exe", "_avpcc.exe",
            "_avpm.exe", "aavgapi.exe", "aawservice.exe", "acaif.exe", "acctmgr.exe", "ackwin32.exe", "aclient.exe",
            "adaware.exe", "advxdwin.exe", "aexnsagent.exe",
            "aexsvc.exe", "aexswdusr.exe", "aflogvw.exe", "afwserv.exe", "agentsvr.exe", "agentw.exe", "ahnrpt.exe",
            "ahnsd.exe", "ahnsdsv.exe", "alertsvc.exe", "alevir.exe",
            "alogserv.exe", "alsvc.exe", "alunotify.exe", "aluschedulersvc.exe", "amon9x.exe", "amswmagt.exe",
            "anti-trojan.exe", "antiarp.exe", "antivirus.exe", "ants.exe",
            "aphost.exe", "apimonitor.exe", "aplica32.exe", "aps.exe", "apvxdwin.exe", "arr.exe", "ashavast.exe",
            "ashbug.exe", "ashchest.exe", "ashcmd.exe", "ashdisp.exe",
            "ashenhcd.exe", "ashlogv.exe", "ashmaisv.exe", "ashpopwz.exe", "ashquick.exe", "ashserv.exe",
            "ashsimp2.exe", "ashsimpl.exe", "ashskpcc.exe", "ashskpck.exe",
            "ashupd.exe", "ashwebsv.exe", "ashdisp.exe", "ashmaisv.exe", "ashserv.exe", "ashwebsv.exe", "asupport.exe",
            "aswdisp.exe", "aswregsvr.exe", "aswserv.exe",
            "aswupdsv.exe", "aswupdsv.exe", "aswwebsv.exe", "aswupdsv.exe", "atcon.exe", "atguard.exe", "atro55en.exe",
            "atupdater.exe", "atwatch.exe", "atwsctsk.exe",
            "au.exe", "aupdate.exe", "aupdrun.exe", "aus.exe", "auto-protect.nav80try.exe", "autodown.exe",
            "autotrace.exe", "autoup.exe", "autoupdate.exe", "avengine.exe",
            "avadmin.exe", "avcenter.exe", "avconfig.exe", "avconsol.exe", "ave32.exe", "avengine.exe", "avesvc.exe",
            "avfwsvc.exe", "avgam.exe", "avgamsvr.exe", "avgas.exe",
            "avgcc.exe", "avgcc32.exe", "avgcsrvx.exe", "avgctrl.exe", "avgdiag.exe", "avgemc.exe", "avgfws8.exe",
            "avgfws9.exe", "avgfwsrv.exe", "avginet.exe", "avgmsvr.exe",
            "avgnsx.exe", "avgnt.exe", "avgregcl.exe", "avgrssvc.exe", "avgrsx.exe", "avgscanx.exe", "avgserv.exe",
            "avgserv9.exe", "avgsystx.exe", "avgtray.exe", "avguard.exe",
            "avgui.exe", "avgupd.exe", "avgupdln.exe", "avgupsvc.exe", "avgvv.exe", "avgw.exe", "avgwb.exe",
            "avgwdsvc.exe", "avgwizfw.exe", "avkpop.exe", "avkserv.exe",
            "avkservice.exe", "avkwctl9.exe", "avltmain.exe", "avmailc.exe", "avmcdlg.exe", "avnotify.exe", "avnt.exe",
            "avp.exe", "avp32.exe", "avpcc.exe", "avpdos32.exe",
            "avpexec.exe", "avpm.exe", "avpncc.exe", "avps.exe", "avptc32.exe", "avpupd.exe", "avscan.exe",
            "avsched32.exe", "avserver.exe", "avshadow.exe", "avsynmgr.exe",
            "avwebgrd.exe", "avwin.exe", "avwin95.exe", "avwinnt.exe", "avwupd.exe", "avwupd32.exe", "avwupsrv.exe",
            "avxmonitor9x.exe", "avxmonitornt.exe", "avxquar.exe",
            "backweb.exe", "bargains.exe", "basfipm.exe", "bd_professional.exe", "bdagent.exe", "bdc.exe", "bdlite.exe",
            "bdmcon.exe", "bdss.exe", "bdsubmit.exe", "beagle.exe",
            "belt.exe", "bidef.exe", "bidserver.exe", "bipcp.exe", "bipcpevalsetup.exe", "bisp.exe", "blackd.exe",
            "blackice.exe", "blink.exe", "blss.exe", "bmrt.exe",
            "bootconf.exe", "bootwarn.exe", "borg2.exe", "bpc.exe", "bpk.exe", "brasil.exe", "bs120.exe", "bundle.exe",
            "bvt.exe", "bwgo0000.exe", "ca.exe", "caav.exe",
            "caavcmdscan.exe", "caavguiscan.exe", "caf.exe", "cafw.exe", "caissdt.exe", "capfaem.exe", "capfasem.exe",
            "capfsem.exe", "capmuamagt.exe", "casc.exe",
            "casecuritycenter.exe", "caunst.exe", "cavrep.exe", "cavrid.exe", "cavscan.exe", "cavtray.exe", "ccapp.exe",
            "ccevtmgr.exe", "cclgview.exe", "ccproxy.exe",
            "ccsetmgr.exe", "ccsetmgr.exe", "ccsvchst.exe", "ccap.exe", "ccapp.exe", "ccevtmgr.exe", "cclaw.exe",
            "ccnfagent.exe", "ccprovsp.exe", "ccproxy.exe", "ccpxysvc.exe",
            "ccschedulersvc.exe", "ccsetmgr.exe", "ccsmagtd.exe", "ccsvchst.exe", "ccsystemreport.exe", "cctray.exe",
            "ccupdate.exe", "cdp.exe", "cfd.exe", "cfftplugin.exe",
            "cfgwiz.exe", "cfiadmin.exe", "cfiaudit.exe", "cfinet.exe", "cfinet32.exe", "cfnotsrvd.exe", "cfp.exe",
            "cfpconfg.exe", "cfpconfig.exe", "cfplogvw.exe",
            "cfpsbmit.exe", "cfpupdat.exe", "cfsmsmd.exe", "checkup.exe", "cka.exe", "clamscan.exe", "claw95.exe",
            "claw95cf.exe", "clean.exe", "cleaner.exe", "cleaner3.exe",
            "cleanpc.exe", "cleanup.exe", "click.exe", "cmdagent.exe", "cmdinstall.exe", "cmesys.exe", "cmgrdian.exe",
            "cmon016.exe", "comhost.exe", "connectionmonitor.exe",
            "control_panel.exe", "cpd.exe", "cpdclnt.exe", "cpf.exe", "cpf9x206.exe", "cpfnt206.exe", "crashrep.exe",
            "csacontrol.exe", "csinject.exe", "csinsm32.exe",
            "csinsmnt.exe", "csrss_tc.exe", "ctrl.exe", "cv.exe", "cwnb181.exe", "cwntdwmo.exe", "cz.exe",
            "datemanager.exe", "dbserv.exe", "dbsrv9.exe", "dcomx.exe",
            "defalert.exe", "defscangui.exe", "defwatch.exe", "deloeminfs.exe", "deputy.exe", "diskmon.exe", "divx.exe",
            "djsnetcn.exe", "dllcache.exe", "dllreg.exe",
            "doors.exe", "doscan.exe", "dpf.exe", "dpfsetup.exe", "dpps2.exe", "drwagntd.exe", "drwatson.exe",
            "drweb.exe", "drweb32.exe", "drweb32w.exe", "drweb386.exe",
            "drwebcgp.exe", "drwebcom.exe", "drwebdc.exe", "drwebmng.exe", "drwebscd.exe", "drwebupw.exe",
            "drwebwcl.exe", "drwebwin.exe", "drwupgrade.exe", "dsmain.exe",
            "dssagent.exe", "dvp95.exe", "dvp95_0.exe", "dwengine.exe", "dwhwizrd.exe", "dwwin.exe", "ecengine.exe",
            "edisk.exe", "efpeadm.exe", "egui.exe", "ekrn.exe",
            "elogsvc.exe", "emet_agent.exe", "emet_service.exe", "emsw.exe", "engineserver.exe", "ent.exe", "era.exe",
            "esafe.exe", "escanhnt.exe", "escanv95.exe",
            "esecagntservice.exe", "esecservice.exe", "esmagent.exe", "espwatch.exe", "etagent.exe", "ethereal.exe",
            "etrustcipe.exe", "evpn.exe", "evtprocessecfile.exe",
            "evtarmgr.exe", "evtmgr.exe", "exantivirus-cnet.exe", "exe.avxw.exe", "execstat.exe", "expert.exe",
            "explore.exe", "f-agnt95.exe", "f-prot.exe", "f-prot95.exe",
            "f-stopw.exe", "fameh32.exe", "fast.exe", "fch32.exe", "fih32.exe", "findviru.exe", "firesvc.exe",
            "firetray.exe", "firewall.exe", "fmon.exe", "fnrb32.exe",
            "fortifw.exe", "fp-win.exe", "fp-win_trial.exe", "fprot.exe", "frameworkservice.exe", "frminst.exe",
            "frw.exe", "fsaa.exe", "fsaua.exe", "fsav.exe", "fsav32.exe",
            "fsav530stbyb.exe", "fsav530wtbyb.exe", "fsav95.exe", "fsavgui.exe", "fscuif.exe", "fsdfwd.exe",
            "fsgk32.exe", "fsgk32st.exe", "fsguidll.exe", "fsguiexe.exe",
            "fshdll32.exe", "fsm32.exe", "fsma32.exe", "fsmb32.exe", "fsorsp.exe", "fspc.exe", "fspex.exe", "fsqh.exe",
            "fssm32.exe", "fwinst.exe", "gator.exe", "gbmenu.exe",
            "gbpoll.exe", "gcascleaner.exe", "gcasdtserv.exe", "gcasinstallhelper.exe", "gcasnotice.exe",
            "gcasserv.exe", "gcasservalert.exe", "gcasswupdater.exe",
            "generics.exe", "gfireporterservice.exe", "ghost_2.exe", "ghosttray.exe", "giantantispywaremain.exe",
            "giantantispywareupdater.exe", "gmt.exe", "guard.exe",
            "guarddog.exe", "guardgui.exe", "hacktracersetup.exe", "hbinst.exe", "hbsrv.exe", "hipsvc.exe",
            "hotactio.exe", "hotpatch.exe", "htlog.exe", "htpatch.exe",
            "hwpe.exe", "hxdl.exe", "hxiul.exe", "iamapp.exe", "iamserv.exe", "iamstats.exe", "ibmasn.exe",
            "ibmavsp.exe", "icepack.exe", "icload95.exe", "icloadnt.exe",
            "icmon.exe", "icsupp95.exe", "icsuppnt.exe", "idle.exe", "iedll.exe", "iedriver.exe", "iface.exe",
            "ifw2000.exe", "igateway.exe", "inetlnfo.exe", "infus.exe",
            "infwin.exe", "inicio.exe", "init.exe", "inonmsrv.exe", "inorpc.exe", "inort.exe", "inotask.exe",
            "intdel.exe", "intren.exe", "iomon98.exe", "ispwdsvc.exe",
            "isuac.exe", "isafe.exe", "isafinst.exe", "issvc.exe", "istsvc.exe", "jammer.exe", "jdbgmrg.exe",
            "jedi.exe", "kaccore.exe", "kansgui.exe", "kansvr.exe",
            "kastray.exe", "kav.exe", "kav32.exe", "kavfs.exe", "kavfsgt.exe", "kavfsrcn.exe", "kavfsscs.exe",
            "kavfswp.exe", "kavisarv.exe", "kavlite40eng.exe",
            "kavlotsingleton.exe", "kavmm.exe", "kavpers40eng.exe", "kavpf.exe", "kavshell.exe", "kavss.exe",
            "kavstart.exe", "kavsvc.exe", "kavtray.exe", "kazza.exe",
            "keenvalue.exe", "kerio-pf-213-en-win.exe", "kerio-wrl-421-en-win.exe", "kerio-wrp-421-en-win.exe",
            "kernel32.exe", "killprocesssetup161.exe", "kis.exe",
            "kislive.exe", "kissvc.exe", "klnacserver.exe", "klnagent.exe", "klserver.exe", "klswd.exe", "klwtblfs.exe",
            "kmailmon.exe", "knownsvr.exe", "kpf4gui.exe",
            "kpf4ss.exe", "kpfw32.exe", "kpfwsvc.exe", "krbcc32s.exe", "kvdetech.exe", "kvolself.exe", "kvsrvxp.exe",
            "kvsrvxp_1.exe", "kwatch.exe", "kwsprod.exe",
            "kxeserv.exe", "launcher.exe", "ldnetmon.exe", "ldpro.exe", "ldpromenu.exe", "ldscan.exe", "leventmgr.exe",
            "livesrv.exe", "lmon.exe", "lnetinfo.exe",
            "loader.exe", "localnet.exe", "lockdown.exe", "lockdown2000.exe", "log_qtine.exe", "lookout.exe",
            "lordpe.exe", "lsetup.exe", "luall.exe", "luau.exe",
            "lucallbackproxy.exe", "lucoms.exe", "lucomserver.exe", "lucoms~1.exe", "luinit.exe", "luspt.exe",
            "makereport.exe", "mantispm.exe", "mapisvc32.exe",
            "masalert.exe", "massrv.exe", "mcafeefire.exe", "mcagent.exe", "mcappins.exe", "mcconsol.exe", "mcdash.exe",
            "mcdetect.exe", "mcepoc.exe", "mcepocfg.exe",
            "mcinfo.exe", "mcmnhdlr.exe", "mcmscsvc.exe", "mcods.exe", "mcpalmcfg.exe", "mcpromgr.exe", "mcregwiz.exe",
            "mcscript.exe", "mcscript_inuse.exe", "mcshell.exe",
            "mcshield.exe", "mcshld9x.exe", "mcsysmon.exe", "mctool.exe", "mctray.exe", "mctskshd.exe", "mcuimgr.exe",
            "mcupdate.exe", "mcupdmgr.exe", "mcvsftsn.exe",
            "mcvsrte.exe", "mcvsshld.exe", "mcwce.exe", "mcwcecfg.exe", "md.exe", "mfeann.exe", "mfevtps.exe",
            "mfin32.exe", "mfw2en.exe", "mfweng3.02d30.exe",
            "mgavrtcl.exe", "mgavrte.exe", "mghtml.exe", "mgui.exe", "minilog.exe", "mmod.exe", "monitor.exe",
            "monsvcnt.exe", "monsysnt.exe", "moolive.exe",
            "mostat.exe", "mpcmdrun.exe", "mpf.exe", "mpfagent.exe", "mpfconsole.exe", "mpfservice.exe", "mpftray.exe",
            "mps.exe", "mpsevh.exe", "mpsvc.exe", "mrf.exe",
            "mrflux.exe", "msapp.exe", "msascui.exe", "msbb.exe", "msblast.exe", "mscache.exe", "msccn32.exe",
            "mscifapp.exe", "mscman.exe", "msconfig.exe", "msdm.exe",
            "msdos.exe", "msiexec16.exe", "mskagent.exe", "mskdetct.exe", "msksrver.exe", "msksrvr.exe", "mslaugh.exe",
            "msmgt.exe", "msmpeng.exe", "msmsgri32.exe",
            "msscli.exe", "msseces.exe", "mssmmc32.exe", "msssrv.exe", "mssys.exe", "msvxd.exe", "mu0311ad.exe",
            "mwatch.exe", "myagttry.exe", "n32scanw.exe", "nsmdemf.exe",
            "nsmdmon.exe", "nsmdreal.exe", "nsmdsch.exe", "naprdmgr.exe", "nav.exe", "navap.navapsvc.exe",
            "navapsvc.exe", "navapw32.exe", "navdx.exe", "navlu32.exe",
            "navnt.exe", "navstub.exe", "navw32.exe", "navwnt.exe", "nc2000.exe", "ncinst4.exe", "msascuil.exe",
            "mbamservice.exe", "mbamtray.exe", "cylancesvc.exe",
            "ndd32.exe", "ndetect.exe", "neomonitor.exe", "neotrace.exe", "neowatchlog.exe", "netalertclient.exe",
            "netarmor.exe", "netcfg.exe", "netd32.exe", "netinfo.exe", "netmon.exe", "netscanpro.exe",
            "netspyhunter-1.2.exe", "netstat.exe", "netutils.exe", "networx.exe",
            "ngctw32.exe", "ngserver.exe", "nip.exe", "nipsvc.exe", "nisoptui.exe", "nisserv.exe", "nisum.exe",
            "njeeves.exe", "nlsvc.exe", "nmain.exe", "nod32.exe",
            "nod32krn.exe", "nod32kui.exe", "normist.exe", "norton_internet_secu_3.0_407.exe", "notstart.exe",
            "npf40_tw_98_nt_me_2k.exe", "npfmessenger.exe",
            "npfmntor.exe", "npfmsg.exe", "nprotect.exe", "npscheck.exe", "npssvc.exe", "nrmenctb.exe", "nsched32.exe",
            "nscsrvce.exe", "nsctop.exe", "nsmdtr.exe",
            "nssys32.exe", "nstask32.exe", "nsupdate.exe", "nt.exe", "ntcaagent.exe", "ntcadaemon.exe",
            "ntcaservice.exe", "ntrtscan.exe", "ntvdm.exe", "ntxconfig.exe",
            "nui.exe", "nupgrade.exe", "nvarch16.exe", "nvc95.exe", "nvcoas.exe", "nvcsched.exe", "nvsvc32.exe",
            "nwinst4.exe", "nwservice.exe", "nwtool16.exe", "nymse.exe",
            "oasclnt.exe", "oespamtest.exe", "ofcdog.exe", "ofcpfwsvc.exe", "okclient.exe", "olfsnt40.exe",
            "ollydbg.exe", "onsrvr.exe", "op_viewer.exe", "opscan.exe",
            "optimize.exe", "ostronet.exe", "otfix.exe", "outpost.exe", "outpostinstall.exe", "outpostproinstall.exe",
            "paamsrv.exe", "padmin.exe", "pagent.exe",
            "pagentwd.exe", "panixk.exe", "patch.exe", "pavbckpt.exe", "pavcl.exe", "pavfires.exe", "pavfnsvr.exe",
            "pavjobs.exe", "pavkre.exe", "pavmail.exe",
            "pavprot.exe", "pavproxy.exe", "pavprsrv.exe", "pavsched.exe", "pavsrv50.exe", "pavsrv51.exe",
            "pavsrv52.exe", "pavupg.exe", "pavw.exe", "pccnt.exe",
            "pccclient.exe", "pccguide.exe", "pcclient.exe", "pccnt.exe", "pccntmon.exe", "pccntupd.exe", "pccpfw.exe",
            "pcctlcom.exe", "pccwin98.exe", "pcfwallicon.exe",
            "pcip10117_0.exe", "pcscan.exe", "pctsauxs.exe", "pctsgui.exe", "pctssvc.exe", "pctstray.exe",
            "pdsetup.exe", "pep.exe", "periscope.exe", "persfw.exe",
            "perswf.exe", "pf2.exe", "pfwadmin.exe", "pgmonitr.exe", "pingscan.exe", "platin.exe", "pmon.exe",
            "pnmsrv.exe", "pntiomon.exe", "pop3pack.exe", "pop3trap.exe",
            "poproxy.exe", "popscan.exe", "portdetective.exe", "portmonitor.exe", "powerscan.exe", "ppinupdt.exe",
            "ppmcativedetection.exe", "pptbc.exe", "ppvstop.exe",
            "pqibrowser.exe", "pqv2isvc.exe", "prevsrv.exe", "prizesurfer.exe", "prmt.exe", "prmvr.exe",
            "programauditor.exe", "proport.exe", "protectx.exe", "psctris.exe",
            "psh_svc.exe", "psimreal.exe", "psimsvc.exe", "pskmssvc.exe", "pspf.exe", "purge.exe", "pview.exe",
            "pviewer.exe", "pxemtftp.exe", "pxeservice.exe",
            "qclean.exe", "qconsole.exe", "qdcsfs.exe", "qoeloader.exe", "qserver.exe", "rapapp.exe", "rapuisvc.exe",
            "ras.exe", "rasupd.exe", "rav7.exe", "rav7win.exe",
            "rav8win32eng.exe", "ravmon.exe", "ravmond.exe", "ravstub.exe", "ravxp.exe", "ray.exe", "rb32.exe",
            "rcsvcmon.exe", "rcsync.exe", "realmon.exe", "reged.exe",
            "remupd.exe", "reportsvc.exe", "rescue.exe", "rescue32.exe", "rfwmain.exe", "rfwproxy.exe", "rfwsrv.exe",
            "rfwstub.exe", "rnav.exe", "rrguard.exe", "rshell.exe",
            "rsnetsvr.exe", "rstray.exe", "rtvscan.exe", "rtvscn95.exe", "rulaunch.exe", "sahookmain.exe",
            "safeboxtray.exe", "safeweb.exe", "sahagent.exescan32.exe",
            "sav32cli.exe", "save.exe", "savenow.exe", "savroam.exe", "savscan.exe", "savservice.exe", "sbserv.exe",
            "scam32.exe", "scan32.exe", "scan95.exe", "scanexplicit.exe",
            "scanfrm.exe", "scanmailoutlook.exe", "scanpm.exe", "schdsrvc.exe", "schupd.exe", "scrscan.exe",
            "seestat.exe", "serv95.exe", "setloadorder.exe",
            "setup_flowprotector_us.exe", "setupguimngr.exe", "setupvameeval.exe", "sfc.exe", "sgssfw32.exe", "sh.exe",
            "shellspyinstall.exe", "shn.exe", "showbehind.exe",
            "shstat.exe", "siteadv.exe", "smoutlookpack.exe", "smc.exe", "smoutlookpack.exe", "sms.exe", "smsesp.exe",
            "smss32.exe", "sndmon.exe", "sndsrvc.exe",
            "soap.exe", "sofi.exe", "softmanager.exe", "spbbcsvc.exe", "spf.exe", "sphinx.exe", "spideragent.exe",
            "spiderml.exe", "spidernt.exe", "spiderui.exe",
            "spntsvc.exe", "spoler.exe", "spoolcv.exe", "spoolsv32.exe", "spyxx.exe", "srexe.exe", "srng.exe",
            "srvload.exe", "srvmon.exe", "ss3edit.exe", "sschk.exe",
            "ssg_4104.exe", "ssgrate.exe", "st2.exe", "stcloader.exe", "stinger.exe", "stopp.exe", "stwatchdog.exe",
            "supftrl.exe", "support.exe", "supporter5.exe",
            "svcgenerichost", "svcharge.exe", "svchostc.exe", "svchosts.exe", "svcntaux.exe", "svdealer.exe",
            "svframe.exe", "svtray.exe", "swdsvc.exe", "sweep95.exe",
            "sweepnet.sweepsrv.sys.swnetsup.exe", "sweepsrv.exe", "swnetsup.exe", "swnxt.exe", "swserver.exe",
            "symlcsvc.exe", "symproxysvc.exe", "symsport.exe", "symtray.exe",
            "symwsc.exe", "sysdoc32.exe", "sysedit.exe", "sysupd.exe", "taskmo.exe", "taumon.exe", "tbmon.exe",
            "tbscan.exe", "tc.exe", "tca.exe", "tclproc.exe", "tcm.exe",
            "tdimon.exe", "tds-3.exe", "tds2-98.exe", "tds2-nt.exe", "teekids.exe", "tfak.exe", "tfak5.exe",
            "tgbob.exe", "titanin.exe", "titaninxp.exe", "tmas.exe",
            "tmlisten.exe", "tmntsrv.exe", "tmpfw.exe", "tmproxy.exe", "tnbutil.exe", "tpsrv.exe", "tracesweeper.exe",
            "trickler.exe", "trjscan.exe", "trjsetup.exe",
            "trojantrap3.exe", "trupd.exe", "tsadbot.exe", "tvmd.exe", "tvtmd.exe", "udaterui.exe", "undoboot.exe",
            "unvet32.exe", "updat.exe", "updtnv28.exe", "upfile.exe",
            "upgrad.exe", "uplive.exe", "urllstck.exe", "usergate.exe", "usrprmpt.exe", "utpost.exe", "v2iconsole.exe",
            "v3clnsrv.exe", "v3exec.exe", "v3imscn.exe",
            "vbcmserv.exe", "vbcons.exe", "vbust.exe", "vbwin9x.exe", "vbwinntw.exe", "vcsetup.exe", "vet32.exe",
            "vet95.exe", "vetmsg.exe", "vettray.exe", "vfsetup.exe",
            "vir-help.exe", "virusmdpersonalfirewall.exe", "vnlan300.exe", "vnpc3000.exe", "vpatch.exe", "vpc32.exe",
            "vpc42.exe", "vpfw30s.exe", "vprosvc.exe",
            "vptray.exe", "vrv.exe", "vrvmail.exe", "vrvmon.exe", "vrvnet.exe", "vscan40.exe", "vscenu6.02d30.exe",
            "vsched.exe", "vsecomr.exe", "vshwin32.exe", "vsisetup.exe",
            "vsmain.exe", "vsmon.exe", "vsserv.exe", "vsstat.exe", "vstskmgr.exe", "vswin9xe.exe", "vswinntse.exe",
            "vswinperse.exe", "w32dsm89.exe", "w9x.exe",
            "watchdog.exe", "webdav.exe", "webproxy.exe", "webscanx.exe", "webtrap.exe", "webtrapnt.exe",
            "wfindv32.exe", "wfxctl32.exe", "wfxmod32.exe", "wfxsnt40.exe",
            "whoswatchingme.exe", "wimmun32.exe", "win-bugsfix.exe", "winactive.exe", "winmain.exe", "winnet.exe",
            "winppr32.exe", "winrecon.exe", "winroute.exe", "winservn.exe",
            "winssk32.exe", "winstart.exe", "winstart001.exe", "wintsk32.exe", "winupdate.exe", "wkufind.exe",
            "wnad.exe", "wnt.exe", "wradmin.exe", "wrctrl.exe",
            "wsbgate.exe", "wssfcmai.exe", "wupdater.exe", "wupdt.exe", "wyvernworksfirewall.exe", "xagt.exe",
            "xagtnotif.exe", "xcommsvr.exe", "xfilter.exe", "xpf202en.exe",
            "zanda.exe", "zapro.exe", "zapsetup3001.exe", "zatutor.exe", "zhudongfangyu.exe", "zlclient.exe", "zlh.exe",
            "zonalm2601.exe", "zonealarm.exe", "cb.exe",
            "msmpeng.exe", "mssense.exe", "csfalconservice.exe", "csfalconcontainer.exe", "redcloak.exe",
            "omniagent.exe", "cramtray.exe", "amsvc.exe", "minionhost.exe",
            "pylumloader.exe", "crssvc.exe"
        };

        public static readonly string[] Admin =
        {
            "mobaxterm.exe", "bash.exe", "git-bash.exe", "mmc.exe", "code.exe", "notepad++.exe", "notepad.exe",
            "cmd.exe",
            "drwatson.exe", "drwtsn32.exe", "drwtsn32.exe", "dumpcap.exe", "ethereal.exe", "filemon.exe", "idag.exe",
            "idaw.exe", "k1205.exe", "loader32.exe",
            "netmon.exe", "netstat.exe", "netxray.exe", "nmwebservice.exe", "nukenabber.exe", "portmon.exe",
            "powershell.exe", "prtg traffic gr.exe",
            "prtg traffic grapher.exe", "prtgwatchdog.exe", "putty.exe", "regmon.exe", "systemeye.exe", "taskman.exe",
            "taskmgr.exe", "tcpview.exe", "totalcmd.exe",
            "trafmonitor.exe", "windbg.exe", "winobj.exe", "wireshark.exe", "wmonavnscan.exe", "wmonavscan.exe",
            "wmonsrv.exe", "regedit.exe", "regedit32.exe",
            "accesschk.exe", "accesschk64.exe", "accessenum.exe", "adexplorer.exe", "adinsight.exe", "adrestore.exe",
            "autologon.exe", "autoruns.exe", "autoruns64.exe",
            "autorunsc.exe", "autorunsc64.exe", "bginfo.exe", "bginfo64.exe", "cacheset.exe", "clockres.exe",
            "clockres64.exe", "contig.exe", "contig64.exe",
            "coreinfo.exe", "ctrl2cap.exe", "dbgview.exe", "desktops.exe", "disk2vhd.exe", "diskext.exe",
            "diskext64.exe", "diskmon.exe", "diskview.exe", "du.exe",
            "du64.exe", "efsdump.exe", "findlinks.exe", "findlinks64.exe", "handle.exe", "handle64.exe", "hex2dec.exe",
            "hex2dec64.exe", "junction.exe", "junction64.exe",
            "ldmdump.exe", "listdlls.exe", "listdlls64.exe", "livekd.exe", "livekd64.exe", "loadord.exe",
            "loadord64.exe", "loadordc.exe", "loadordc64.exe",
            "logonsessions.exe", "logonsessions64.exe", "movefile.exe", "movefile64.exe", "notmyfault.exe",
            "notmyfault64.exe", "notmyfaultc.exe", "notmyfaultc64.exe",
            "ntfsinfo.exe", "ntfsinfo64.exe", "pagedfrg.exe", "pendmoves.exe", "pendmoves64.exe", "pipelist.exe",
            "pipelist64.exe", "portmon.exe", "procdump.exe",
            "procdump64.exe", "procexp.exe", "procexp64.exe", "procmon.exe", "psexec.exe", "psexec64.exe", "psfile.exe",
            "psfile64.exe", "psgetsid.exe", "psgetsid64.exe",
            "psinfo.exe", "psinfo64.exe", "pskill.exe", "pskill64.exe", "pslist.exe", "pslist64.exe", "psloggedon.exe",
            "psloggedon64.exe", "psloglist.exe", "pspasswd.exe",
            "pspasswd64.exe", "psping.exe", "psping64.exe", "psservice.exe", "psservice64.exe", "psshutdown.exe",
            "pssuspend.exe", "pssuspend64.exe", "rammap.exe",
            "regdelnull.exe", "regdelnull64.exe", "regjump.exe", "ru.exe", "ru64.exe", "sdelete.exe", "sdelete64.exe",
            "shareenum.exe", "shellrunas.exe", "sigcheck.exe",
            "sigcheck64.exe", "streams.exe", "streams64.exe", "strings.exe", "strings64.exe", "sync.exe", "sync64.exe",
            "sysmon.exe", "sysmon64.exe", "tcpvcon.exe",
            "tcpview.exe", "testlimit.exe", "testlimit64.exe", "vmmap.exe", "volumeid.exe", "volumeid64.exe",
            "whois.exe", "whois64.exe", "winobj.exe", "zoomit.exe",
            "keepass.exe", "1password.exe", "lastpass.exe"
        };



        public static readonly string[] SIEM =
        {
            "splunkd.exe", "winlogbeat.exe", "wincollect.exe"
        };

    }
}