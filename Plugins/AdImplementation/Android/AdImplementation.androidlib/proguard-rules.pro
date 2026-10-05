# Regles R8 / ProGuard d'AdImplementation, appliquees automatiquement a l'app qui utilise ce package
# (consumerProguardFiles dans build.gradle). Inutile de les recopier dans le proguard-user.txt des projets.

# ----------------------------------------------------------------------------------------------
# ADMOST
# ----------------------------------------------------------------------------------------------
-keepattributes Exceptions, InnerClasses
-dontwarn admost.sdk.**
-keep class admost.sdk.** { *; }
-dontwarn admost.adserver.**
-keep class admost.adserver.** { *; }
-dontwarn com.amr.unity.**
-keep class com.amr.unity.** { *; }

# ----------------------------------------------------------------------------------------------
# REGIES : SDK CHERCHES PAR LEUR NOM
# AdMost verifie la presence de chaque SDK de regie en cherchant ses classes PAR LEUR NOM (reflexion).
# Si R8 les renomme, AdMost ecarte la regie sans erreur visible : « SDK class files not found : VUNGLE »
# et « ... : CHARTBOOST » dans ADMOST_LOG (constate le 05/10/2026 sur Piggy Prize).
# Chartboost 9.x n'embarque aucune regle consumer ; celles de Vungle 7.x laissent renommer les classes.
# ----------------------------------------------------------------------------------------------
-dontwarn com.chartboost.**
-keep class com.chartboost.** { *; }
-dontwarn com.vungle.**
-keep class com.vungle.** { *; }
