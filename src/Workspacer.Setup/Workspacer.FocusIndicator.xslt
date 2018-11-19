<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                exclude-result-prefixes="xsl wix">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

  <xsl:strip-space elements="*"/>

  <xsl:key name="ToRemove"
           match="wix:Component[contains(wix:File/@Source, '.pdb') or contains(wix:File/@Source, '.xml') or contains(wix:File/@Source, 'Workspacer.Shared.dll') or contains(wix:File/@Source, 'NLog.dll')]"
           use="@Id" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="*[self::wix:Component or self::wix:ComponentRef]
                        [key('ToRemove', @Id)]" />
</xsl:stylesheet>
