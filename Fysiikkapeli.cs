using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// <summary>
/// 
/// </summary>
public class HT2 : PhysicsGame
{
    /// Pelaajan liike

    Vector nopeusYlos = new Vector(0, 2000);
    Vector nopeusAlas = new Vector(0, -2000);
    Vector nopeusVasemmalle = new Vector(-2000, 0);
    Vector nopeusOikealle = new Vector(2000, 0);

    DoubleMeter elamaLaskuri;
    DoubleMeter karkkiLaskuri;

    SoundEffect karkkiAani = LoadSoundEffect("powerUp2.wav");

    public override void Begin()
    {
        Level.Background.Image = LoadImage("Tausta.png");
        Level.CreateBorders();
        Mouse.IsCursorVisible = true;



        PhysicsObject pelaaja = LuoNelikulmio(this, "pelaaja1", -350, -350);
        PhysicsObject vesa = LuoVesa(this, "vesa", 0, 0);

        pelaaja.Image = LoadImage("ukko.png");
        vesa.Image = LoadImage("vessuli.png");


        AddCollisionHandler(pelaaja, "kynis", kynaOsuuPelaajaan); ;
        AddCollisionHandler(pelaaja, "karkkis", pelaajaTormaaKarkkiin);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä napit");
        Keyboard.Listen(Key.Up, ButtonState.Down, LyoUkkoa, "Liikuta ylös", pelaaja, nopeusYlos);
        Keyboard.Listen(Key.Down, ButtonState.Down, LyoUkkoa, "Liikuta alas", pelaaja, nopeusAlas);
        Keyboard.Listen(Key.Right, ButtonState.Down, LyoUkkoa, "Liikuta oikealle", pelaaja, nopeusOikealle);
        Keyboard.Listen(Key.Left, ButtonState.Down, LyoUkkoa, "Liikuta vasemmalle", pelaaja, nopeusVasemmalle);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Lopeta peli");

        // MediaPlayer.Play("KarkkiPeli_01.mp3");
        // MediaPlayer.IsRepeating = true;

        Timer synnytaKynia = new Timer();
        synnytaKynia.Interval = 1.5;
        synnytaKynia.Timeout += LuoKyna;
        synnytaKynia.Start();

        Timer synnytaKarkkeja = new Timer();
        synnytaKarkkeja.Interval = 4.0;
        synnytaKarkkeja.Timeout += LuoKarkki;
        synnytaKarkkeja.Start();

        /*Timer pisteet = new Timer();
        pisteet.Interval = 1.0;
        pisteet.Timeout += lisaaPiste;
        pisteet.Start();*/

        LuoElamaLaskuri();
        LuoAikaLaskuri();
        LuoKarkkiLaskuri();



    }

    public static PhysicsObject LuoNelikulmio(PhysicsGame peli, string tunniste, double x, double y)
    {
        PhysicsObject ukko = new PhysicsObject(70, 100, Shape.Rectangle);
        ukko.Color = Color.Black;

        // ukko.Hit(suunta);
        ukko.Tag = tunniste;
        // ukko.Mass = 1.0;
        peli.Add(ukko);
        ukko.LinearDamping = 0.93;
        ukko.Restitution = 0;
        ukko.AngularDamping = 0.1;
        ukko.MaxVelocity = 40000;
        ukko.X = x;
        ukko.Y = y;


        return ukko;
    }

    public static PhysicsObject LuoVesa(PhysicsGame peli, string tunniste, double x, double y)
    {
        PhysicsObject ukko = new PhysicsObject(200, 200, Shape.Rectangle);
        ukko.Color = Color.Black;

        // ukko.Hit(suunta);
        ukko.Tag = tunniste;
        // ukko.Mass = 1.0;
        peli.Add(ukko, -1);
        ukko.LinearDamping = 0.93;
        ukko.Restitution = 0;
        ukko.AngularDamping = 0.9;
        ukko.MaxVelocity = 40000;
        ukko.X = x;
        ukko.Y = y;
        ukko.IgnoresCollisionResponse = true;

        return ukko;
    }

    public static void LyoUkkoa(PhysicsObject ukko, Vector suunta)
    {
        ukko.Push(suunta);
    }


    void LuoKarkki()
    {
        PhysicsObject karkki = new PhysicsObject(35, 35);
        karkki.Color = Color.Red;
        Add(karkki);
        karkki.Y = 0;
        karkki.X = 0;
        karkki.Tag = "karkkis";
        karkki.Image = LoadImage("karkkiHR.png");
        Vector suunta = RandomGen.NextVector(300, 400);
        karkki.Hit(suunta);
        karkki.LifetimeLeft = TimeSpan.FromSeconds(10.0);
    }

    void pelaajaTormaaKarkkiin(PhysicsObject pelaaja, PhysicsObject karkki)
    {
        pelaaja.Color = new Color(RandomGen.NextInt(0, 255), RandomGen.NextInt(0, 255), RandomGen.NextInt(0, 255));
        Remove(karkki);
        karkkiLaskuri.Value += 1;
    }


    void LuoKyna()
    {
        PhysicsObject kyna = new PhysicsObject(40, 20);
        kyna.Color = Color.Red;
        Add(kyna);
        kyna.Y = 0;
        kyna.X = 0;
        kyna.Tag = "kynis";
        kyna.Image = LoadImage("kynaHT.png");
        Vector suunta = RandomGen.NextVector(300, 400);
        kyna.Hit(suunta);
        kyna.LifetimeLeft = TimeSpan.FromSeconds(10.0);
    }

    void kynaOsuuPelaajaan(PhysicsObject pelaaja, PhysicsObject kyna)
    {
        Explosion rajahdys = new Explosion(kyna.Width * 2);
        rajahdys.Position = kyna.Position;
        rajahdys.UseShockWave = false;
        this.Add(rajahdys);
        Remove(kyna);
        elamaLaskuri.Value -= 1;
    }

    void LuoAikaLaskuri()
    {
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.Black;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.X = Screen.Right - 150;
        aikaNaytto.Y = Screen.Top - 20;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        Add(aikaNaytto, 2);
    }

    void LuoElamaLaskuri()
    {
        elamaLaskuri = new DoubleMeter(5);
        elamaLaskuri.MaxValue = 5;
        elamaLaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamaPalkki = new ProgressBar(150, 20);
        elamaPalkki.X = Screen.Center.X;
        elamaPalkki.Y = Screen.Top - 20;
        elamaPalkki.BindTo(elamaLaskuri);
        elamaPalkki.Image = LoadImage("emptyHearts.png");
        elamaPalkki.BarImage = LoadImage("fullHearts.png");
        Add(elamaPalkki);
    }

    void LuoKarkkiLaskuri()
    {
        karkkiLaskuri = new DoubleMeter(0);
        karkkiLaskuri.MaxValue = 5;
        karkkiLaskuri.UpperLimit += ViisiKarkkia;

        ProgressBar karkkiPalkki = new ProgressBar(150, 20);
        karkkiPalkki.X = Screen.Center.X;
        karkkiPalkki.Y = Screen.Top - 60;
        karkkiPalkki.BindTo(karkkiLaskuri);
        karkkiPalkki.Image = LoadImage("tyhjaHorizontal.png");
        karkkiPalkki.BarImage = LoadImage("taysiHorizontal.png");
        Add(karkkiPalkki);
    }
    void ElamaLoppui()
    {
        MessageDisplay.Add("Elämät loppuivat, voi voi.");
        Exit();
    }

    void ViisiKarkkia()
    {
        elamaLaskuri.Value += 1;
        karkkiLaskuri.Value = 0;
        karkkiAani.Play();
        MessageDisplay.Add("Sait yhden elämän lisää!");
    }


}