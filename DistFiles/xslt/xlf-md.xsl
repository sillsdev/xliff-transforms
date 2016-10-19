<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        xlf-md.xsl
    # Purpose:     Translate XLiff format into Markdown (md)
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/09/27
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xliff="urn:oasis:names:tc:xliff:document:2.0"
    version="2.0">

    <xsl:param name="outputBase"/>
    <xsl:output omit-xml-declaration="yes" />

    <xsl:template match="/">
        <xsl:apply-templates select="//file | //xliff:file"/>
    </xsl:template>

    <xsl:template match="file | xliff:file">
        <xsl:result-document href="{concat($outputBase,@original)}" omit-xml-declaration="yes" method="text">
            <xsl:apply-templates select=".//unit | .//xliff:unit"/>
        </xsl:result-document>
    </xsl:template>

    <xsl:template match="unit | xliff:unit">
        <xsl:for-each select="*">
            <xsl:choose>
                <xsl:when test="target | xliff:target">
                    <xsl:variable name="val" select="target | xliff:target"/>
                    <xsl:value-of select="replace(normalize-space($val),'\\t','&#9;')"/>
                    <xsl:if test="position() != last()">
                        <xsl:text> </xsl:text>
                    </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="source | xliff:source"/>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:for-each>
        <xsl:text>&#13;&#10;</xsl:text>
    </xsl:template>

</xsl:stylesheet>