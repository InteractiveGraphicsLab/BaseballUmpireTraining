using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeZoneProperty
{
    public Vector3 centerPosition { get; private set; }
    public Vector2 size { get; private set; }
    public int vertDivNum { get; private set; }
    public int horiDivNum { get; private set; }
    public int vertOffset { get; private set; }
    public int horiOffset { get; private set; }
    public Vector2 panelSize { get; private set; }
    public int vertPanelNum { get; private set; }
    public int horiPanelNum { get; private set; }
    public Vector2 zoneSize { get; private set; }

    public StrikeZoneProperty(StrikeZoneProperty s)
    : this(s.centerPosition, s.size, s.vertDivNum, s.horiDivNum, s.vertOffset, s.horiOffset){}

    public StrikeZoneProperty(Vector3 center, float width, float height, int vertDiv, int horiDiv, int vertOff, int horiOff)
    : this(center, new Vector2(width, height), vertDiv, horiDiv, vertOff, horiOff){}

    public StrikeZoneProperty(Vector3 center, Vector2 zoneSize, int vertDiv, int horiDiv, int vertOff, int horiOff)
    {
        this.centerPosition = center;
        this.size = zoneSize;
        this.vertDivNum = vertDiv;
        this.horiDivNum = horiDiv;
        this.vertOffset = vertOff;
        this.horiOffset = horiOff;
        this.panelSize = new Vector2(zoneSize.x / horiDivNum, zoneSize.y / vertDivNum);
        this.vertPanelNum = vertDivNum + vertOffset * 2;
        this.horiPanelNum = horiDivNum + horiOffset * 2;
        this.zoneSize = new Vector2(panelSize.x * horiPanelNum, panelSize.y * vertPanelNum);
    }

    public bool IsStrike(int line, int column)
    {
        return this.horiOffset <= line && line <= this.horiOffset + this.horiDivNum - 1
                && this.vertOffset <= column && column <= this.vertOffset + this.vertDivNum - 1;
    }

    public bool IsInZone(int line, int column)
    {
        return (0 <= line && line < this.horiDivNum + this.horiOffset * 2) && (0 <= column && column < this.vertDivNum + this.vertOffset * 2);
    }
}
