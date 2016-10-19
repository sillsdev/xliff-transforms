<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        xlf-tmx.xsl
    # Purpose:     Translate XLiff format into TMX
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/09/08
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xliff="urn:oasis:names:tc:xliff:document:2.0"
    version="2.0">

    <xsl:output indent="yes"/>
    <xsl:variable name="src" select="string(/*/@srcLang)"/>
    <xsl:variable name="trg" select="string(/*/@trgLang)"/>

    <xsl:template match="/">
        <xsl:apply-templates select="//file | //xliff:file"/>
    </xsl:template>

    <xsl:template match="file | xliff:file">
        <xsl:result-document href="{@original}">
            <xsl:element name="tmx">
                <xsl:attribute name="version">1.4</xsl:attribute>
                <xsl:element name="header">
                    <xsl:attribute name="srclang">
                        <xsl:value-of select="$trg"/>
                    </xsl:attribute>
                    <xsl:attribute name="adminlang">
                        <xsl:value-of select="$src"/>
                    </xsl:attribute>
                    <xsl:attribute name="creationtool">Palaso Localization Manager</xsl:attribute>
                    <xsl:attribute name="creationtoolversion">2.0.30.0</xsl:attribute>
                    <xsl:attribute name="segtype">block</xsl:attribute>
                    <xsl:attribute name="datatype">unknown</xsl:attribute>
                    <xsl:attribute name="o-tmf">PalasoTMXUtils</xsl:attribute>
                    <xsl:element name="prop">
                        <xsl:attribute name="type">x-appversion</xsl:attribute>
                        <xsl:text>3.1.000.0</xsl:text>
                    </xsl:element>
                    <xsl:element name="prop">
                        <xsl:attribute name="type">x-hardlinebreakreplacement</xsl:attribute>
                        <xsl:text>\n</xsl:text>
                    </xsl:element>
                </xsl:element>
                <xsl:element name="body">
                    <xsl:apply-templates select="//unit | //xliff:unit"/>
                </xsl:element>
            </xsl:element>
            </xsl:result-document>
    </xsl:template>

    <xsl:template match="unit | xliff:unit">
        <xsl:element name="tu">
            <xsl:attribute name="tuid">
                <xsl:value-of select="@id"/>
            </xsl:attribute>
            <xsl:if test="count(ignorable) + count(xliff:ignorable) > 0">
                <xsl:element name="prop">
                    <xsl:attribute name="type">x-nolongerused</xsl:attribute>
                    <xsl:text>true</xsl:text>
                </xsl:element>
            </xsl:if>
            <xsl:for-each select="notes/note | xliff:notes/xliff:note">
                <xsl:choose>
                    <xsl:when test="contains(., '&lt;')">
                        <xsl:text>&#10;         </xsl:text>
                        <xsl:value-of select="." disable-output-escaping="yes"/>
                        <xsl:text>&#10;         </xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:element name="note">
                            <xsl:apply-templates/>
                        </xsl:element>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:for-each>
            <xsl:element name="tuv">
                <xsl:attribute name="xml:lang">
                    <xsl:value-of select="$src"/>
                </xsl:attribute>
                <xsl:element name="seg">
                    <xsl:for-each select=".//source | .//xliff:source">
                        <xsl:value-of select="."/>
                    </xsl:for-each>
                </xsl:element>
            </xsl:element>
            <xsl:element name="tuv">
                <xsl:attribute name="xml:lang">
                    <xsl:value-of select="$trg"/>
                </xsl:attribute>
                <xsl:element name="seg">
                    <xsl:for-each select=".//target | .//xliff:target">
                        <xsl:value-of select="."/>
                    </xsl:for-each>
                </xsl:element>
            </xsl:element>
        </xsl:element>
    </xsl:template>

</xsl:stylesheet>