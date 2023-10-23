using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 셀 생성용 클래스
/// </summary>
public class WorkArea : MonoBehaviour
{
    public bool activeControll;
    bool active = false;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            active = value;
            if (active)
            {
                positioning.Invoke();
                active = false;
                activeControll =false;
            }
        }
    }

    public List<CellData> cells = new List<CellData>();

    public Vector2Int size;

    [Range(0, 38)]
    int horizonCell;
    public int HorizonCell
    {
        get
        {
            return horizonCell;
        }
        set
        {
            if (horizonCell != value)
            {
                horizonCell = value;
            }
        }
    }

    [Range(0, 12)]
    int verticalCell;
    public int VerticalCell
    {
        get
        {
            return verticalCell;
        }
        set
        {
            if (verticalCell != value)
            {
                verticalCell = value;
            }
        }
    }

    public GameObject tileimage;

    public Vector2 StartPosition;
    Vector2 startPos;
    public Vector2 StartPos
    {
        get
        {
            return startPos;
        }
        set
        {
            if (startPos != value)
            {
                startPos = value;
            }
        }
    }

    Action positioning;

    public float space = 0.5f;

    private void OnEnable()
    {
        positioning += TileSetting;
        TileSetting();
    }

    void TileSetting()
    {
        HorizonCell = size.x;
        VerticalCell = size.y;
        StartPos = StartPosition;

        if (transform.childCount != 0)
        {
            cells.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject, 1f);
            }
        }
        if (VerticalCell < 20)
        {
            int makeingnum = 0;
            for (int i = 0; i < VerticalCell; i++)
            {
                float Y = startPos.y + (i * space);

                for (int j = 0; j < HorizonCell; j++)
                {
                    float X = startPos.x - (j * space);
                    Vector3 workpos = new Vector3(X, Y, 0);
                    GameObject cellobject = Instantiate(tileimage, workpos, Quaternion.identity);
                    cellobject.transform.parent = this.transform;
                    cells.Add(cellobject.GetComponent<CellData>());
                    cells[makeingnum].number = makeingnum;
                    cells[makeingnum].CellState = cellState.Empty;
                    makeingnum++;
                }
            }
        }
        else
        {
            int makeingnum = 0;
            for (int i = 0; i < 19; i++)
            {
                float Y = startPos.y + (i * space);

                for (int j = 0; j < HorizonCell; j++)
                {
                    float X = startPos.x - (j * space);
                    Vector3 workpos = new Vector3(X, Y, 0);
                    GameObject cellobject = Instantiate(tileimage, workpos, Quaternion.identity);
                    cellobject.transform.parent = this.transform;
                    cells.Add(cellobject.GetComponent<CellData>());
                    cells[makeingnum].number = makeingnum;
                    cells[makeingnum].CellState = cellState.Empty;
                    makeingnum++;
                }
            }
            for (int i = 0; i < VerticalCell - 19; i++)
            {
                float Y = startPos.y + (i * space);

                for (int j = 0; j < HorizonCell; j++)
                {
                    float X = (-startPos.x + ((HorizonCell - 1) * space)) - (j * space);
                    Vector3 workpos = new Vector3(X, Y, 0);
                    GameObject cellobject = Instantiate(tileimage, workpos, Quaternion.identity);
                    cellobject.transform.parent = this.transform;
                    cells.Add(cellobject.GetComponent<CellData>());
                    cells[makeingnum].number = makeingnum;
                    cells[makeingnum].CellState = cellState.Empty;
                    makeingnum++;
                }
            }
        }
    }
    private void Update()
    {
        Active = activeControll;
    }
}


