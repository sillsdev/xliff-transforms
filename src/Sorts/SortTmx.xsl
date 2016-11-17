<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        SortTmx.xsl
    # Purpose:     Sort translation memory by tuid
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2016/11/17
    # Copyright:   (c) 2016 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

    <xsl:output method="xml" encoding="utf-8" indent="yes"/>

    <!-- Copy nodes -->
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="body">
        <xsl:copy>
		    <xsl:text>&#10;    </xsl:text>
            <xsl:apply-templates select="tu">
                <xsl:sort select="@tuid" order="ascending"/>
            </xsl:apply-templates>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="tu">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*"/>
        </xsl:copy>
		<xsl:text>&#10;    </xsl:text>
    </xsl:template>
</xsl:stylesheet>