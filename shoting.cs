using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

class Sprite
{
    public int x; 
    public int y;

    public Sprite( int init_x, int init_y ) 
    {
        x = init_x;
        y = init_y;
    }

    public bool Intersect( Rectangle pos1, Rectangle pos2 )
    {
        return( ( pos1.X + pos1.Width >= pos2.X ) && ( pos1.X <= pos2.X + pos2.Width )
            &&  ( pos1.Y + pos1.Height >= pos2.Y ) && ( pos1.Y <= pos2.Y + pos2.Height ) );
    }
}

class Enemy : Sprite
{
    public Rectangle mPos;
    public int mRestCount = 1;
    public int mCoolTime = 25;
    public int mDX = 30;

    public Enemy( int init_x, int init_y ) : base( init_x, init_y )
    {        
        mPos = new Rectangle( x, y, Program.SCALE, Program.SCALE );
    }

    public void Tick()
    {
        mPos = new Rectangle( x, y, Program.SCALE, Program.SCALE );
    }
}

class Player : Sprite
{
	public Rectangle mPos;

	int mWidth;
	int mHeight;

    public Player( int init_x, int init_y, int w, int h ) : base( init_x, init_y )
    {
		mPos = new Rectangle( x + 5, y, w - 10, h );
		mWidth = w;
		mHeight = h;
    }

	public void Draw( Graphics g )
	{
		g.DrawImage( Program.playerImg, x, y, Program.SCALE, Program.SCALE );
	}

	public void Tick()
	{
		mPos = new Rectangle( x + 5, y, mWidth - 10, mHeight );
	}
}

class Bullet : Sprite
{
	public readonly int B_WIDTH = 5;
	public readonly int B_HEIGHT = 30;

    public Rectangle mPos;
    public int dy;

    public Bullet( int init_x, int init_y, int init_dy ) : base( init_x, init_y )
    {
        dy = init_dy;
    }

	public void Draw( Graphics g )
	{
		g.FillRectangle( Brushes.Black, x, y, B_WIDTH, B_HEIGHT );
	}

    public void Tick()
    {
        mPos = new Rectangle( x, y, B_WIDTH, B_HEIGHT );
    }
}

class Program : Form
{
    public static readonly int WIDTH = 300;
    public static readonly int HEIGHT = 300;
    public static readonly int SCALE = 30;
    public static readonly int FPS = 16;
    public static readonly int ENEMY_OFFSET_X = 25;

    public static Bitmap enemyImg = new Bitmap( "enemy.png" );
    public static Bitmap playerImg = new Bitmap( "player.png" );

    List<Enemy> enemy = new List<Enemy>(){};
    List<Bullet> eBullet = new List<Bullet>(){};

    List<Bullet> bullet = new List<Bullet>(){};
	Player player = new Player( 150, 300 - 30 * 2, SCALE, SCALE );

    int mScoer = 0;
	int mCoolTime = 0;

    int eCoolTime = 0;

    bool key_r, key_l;
    bool mGameClear, mGameOver;

    Program()
    {
        ClientSize = new Size( WIDTH, HEIGHT );
        DoubleBuffered = true;

        for( int x = 25; x < 256; x += 30 ) {
            enemy.Add( new Enemy( x, 50 ) );
        }

        Task.Run( () =>{
            while( true ) {
                onTimer();
                Task.Delay( FPS ).Wait();
            }
        });
    }

    void onTimer() 
    {
        int speed = 3;

        if( ( mGameClear ) || ( mGameOver ) ) {
            eBullet.Clear();
            return;
        }

		player.Tick();

        if( enemy.Count <= 0 ) {
            mGameClear = true;
        }

		if( mCoolTime > 0 ) {
			mCoolTime--;
		}

        if( key_l ) player.x -= speed;
        if( key_r ) player.x += speed;

 		for( int i = 0; i < bullet.Count; i++ ) {
            bool remove = false;

            bullet[ i ].Tick();
			bullet[ i ].y += bullet[ i ].dy;

			if( bullet[ i ].y < 0 ) {
                remove = true;
				bullet.RemoveAt( i );
			}

            if( !remove ) {
                bool eRemove = false;

                for( int n = 0; n < enemy.Count; n++ ) {
                    enemy[ i ].Tick();

                    if( bullet[ i ].Intersect( bullet[ i ].mPos, enemy[ n ].mPos ) ) {
                        enemy.RemoveAt( n );
                        eRemove = true;
                    }
                }

                if( eRemove ) {
                    bullet.RemoveAt( i );
                }
            }
		}

        if( eCoolTime > 0 ) {
            eCoolTime--;
        }
        
        if( eCoolTime <= 0 ) {
            for( int n = 0; n < enemy.Count; n++ ) {            
                Bullet b = new Bullet( 0, 0, 0 );

                if( ( player.x + player.mPos.Width >= enemy[ n ].x )
                &&  ( player.x <= enemy[ n ].x + SCALE ) ) {
                    eBullet.Add( new Bullet( enemy[ n ].x + 13, enemy[ n ].y + 10, 3 ) ); 
                    break;
                }
            } 

            eCoolTime = 25;
        }

        for( int i = 0; i < eBullet.Count; i++ ) {
            eBullet[ i ].Tick();
            eBullet[ i ].y += eBullet[ i ].dy;

			if( eBullet[ i ].y + eBullet[ i ].B_HEIGHT >= WIDTH ) {
				eBullet.RemoveAt( i );
			}

            if( eBullet[ i ].Intersect( eBullet[ i ].mPos, player.mPos ) ) {
                eBullet.Clear();
                mGameOver = true;
            }
        }

        Invalidate();
    }

    protected override void OnPaint( PaintEventArgs e )
    {
        Graphics g = e.Graphics;

        if( mGameClear ) {
            g.DrawString( "GAMECLEAR!!", new Font( "pixelMplus10", 28 ), new SolidBrush( Color.Black ), WIDTH / 2 - 110, HEIGHT / 2 - 40 );
            g.DrawString( "HI SCOER " + mScoer, new Font( "pixelMplus10", 18 ), new SolidBrush( Color.Black ), WIDTH / 2 - 60, HEIGHT / 2 );                
        } else if( mGameOver ) {
            g.DrawString( "GAMEOVER...", new Font( "pixelMplus10", 28 ), new SolidBrush( Color.Black ), WIDTH / 2 - 110, HEIGHT / 2 - 40 );
            g.DrawString( "HI SCOER " + mScoer, new Font( "pixelMplus10", 18 ), new SolidBrush( Color.Black ), WIDTH / 2 - 60, HEIGHT / 2 );                
        }

        g.DrawString( "SCOER " + mScoer, new Font( "pixelMplus10", 14 ), new SolidBrush( Color.Black ), 10, 10 );    

        player.Draw( g );

		for( int i = 0; i < bullet.Count; i++ ) {
			bullet[ i ].Draw( g );
		}

        for( int i = 0; i < eBullet.Count; i++ ) {
            eBullet[ i ].Draw( g );
        }
        
        for( int i = 0; i < enemy.Count; i++ ) {
            g.DrawImage( enemyImg, enemy[ i ].x, enemy[ i ].y, SCALE, SCALE );
        }
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
        switch( e.KeyCode ) {
            case Keys.Right:
                key_r = true;
                break;

            case Keys.Left:
                key_l = true;
                break;

            case Keys.Space:
                if( mCoolTime <= 0 ) {
					bullet.Add( new Bullet( player.x + 12, player.y - 10, -3 ) );
					mCoolTime = 10;
				}

                break;
        }
    }

    protected override void OnKeyUp( KeyEventArgs e )
    {
        switch( e.KeyCode ) {
            case Keys.Right:
                key_r = false;
                break;

            case Keys.Left:
                key_l = false;
                break;
        }
    }

    static void Main()
    {
        Application.Run( new Program() );
    }
}
