<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        SortXlf.xsl
    # Purpose:     Sort XLIFF translation memory by id
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/11/17
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:xliff="urn:oasis:names:tc:xliff:document:2.0"
    version="1.0">

    <xsl:output method="xml" encoding="utf-8" indent="yes"/>

    <!-- Copy nodes -->
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="xliff:file">
        <xsl:copy>
		    <xsl:apply-templates select="@*"/>
			<xsl:text>&#10;    </xsl:text>
            <xsl:apply-templates select="xliff:unit">
                <xsl:sort select="@id" order="ascending"/>
            </xsl:apply-templates>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="xliff:unit">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
		<xsl:text>&#10;    </xsl:text>
    </xsl:template>
</xsl:stylesheet>