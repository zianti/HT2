﻿using System;
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
    Vector nopeusYlos = new Vector(0, 2000);
    Vector nopeusAlas = new Vector(0, -2000);
    Vector nopeusVasemmalle = new Vector(-2000, 0);
    Vector nopeusOikealle = new Vector(2000, 0);

    DoubleMeter elamaLaskuri;
    DoubleMeter karkkiLaskuri;

    SoundEffect karkkiAani = LoadSoundEffect("powerUp2.wav");

    public override void Begin()
    {
        Valikko();
    }

    void Valikko()
    {
        MultiSelectWindow alkuValikko = new MultiSelectWindow("Pelin alkuvalikko",
"Aloita peli", "Lopeta");
        Add(alkuValikko);
        alkuValikko.AddItemHandler(0, AloitaPeli);
        alkuValikko.AddItemHandler(1, Exit);
    }

    /// <summary>
    /// Luo kentän, pelihahmot, asettaa napit, aloittaa ajastimia ja laskureita.
    /// </summary>
    void AloitaPeli()
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
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

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

        LuoElamaLaskuri();
        LuoAikaLaskuri();
        LuoKarkkiLaskuri();

    }

    /// <summary>
    /// Luo pelaajan hahmon
    /// </summary>
    /// <param name="peli">pelikenttä</param>
    /// <param name="tunniste">sana, jolla tietää mistä fysiikkaoliosta kyse</param>
    /// <param name="x">fysiikkaolion x-koordinaatti</param>
    /// <param name="y">fysiikkaolion y-koordinaatti</param>
    /// <returns></returns>
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

    /// <summary>
    /// Luo vihollishahmon
    /// </summary>
    /// <param name="peli">pelikenttä</param>
    /// <param name="tunniste">sana, jolla tietää mistä fysiikkaoliosta kyse</param>
    /// <param name="x">fysiikkaolion x-koordinaatti</param>
    /// <param name="y">fysiikkaolion y-koordinaatti</param>
    /// <returns></returns>
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

    /// <summary>
    /// Kun pelaaja törmää karkkiin karkkipalkki täyttyy. Viisi karkkia kerätessä saa yhden elämän lisää.
    /// </summary>
    /// <param name="pelaaja">Pelaaja on olio, jota liikutetaan. Sillä on tarkoitus kerätä karkkeja</param>
    /// <param name="karkki">Karkki on esine, joka liikkuu ja niitä täytyy kerätä</param>
    void pelaajaTormaaKarkkiin(PhysicsObject pelaaja, PhysicsObject karkki)
    {
        pelaaja.Color = new Color(RandomGen.NextInt(0, 255), RandomGen.NextInt(0, 255), RandomGen.NextInt(0, 255));
        Remove(karkki);
        karkkiLaskuri.Value += 1;
    }

    /// <summary>
    /// Aliohjelma tekee liikkuvan kynän, jota täytyy väistellä.
    /// </summary>
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

    /// <summary>
    /// Kynän osuessa pelaajaan tapahtuu räjähdys ja pelaajalta lähtee yksi elämä.
    /// </summary>
    /// <param name="pelaaja">Pelaaja on olio, jolla peliä pelataan.</param>
    /// <param name="kyna">Kynä on esine, jota on tarkoitus väistää.</param>
    void kynaOsuuPelaajaan(PhysicsObject pelaaja, PhysicsObject kyna)
    {
        Explosion rajahdys = new Explosion(kyna.Width * 2);
        rajahdys.Position = kyna.Position;
        rajahdys.UseShockWave = false;
        this.Add(rajahdys);
        Remove(kyna);
        elamaLaskuri.Value -= 1;
    }

    /// <summary>
    /// Aikalaskurista näkee miten pitkään on pysynyt elossa. Mitä pitempään pysyy elossa, sitä parempi tulos on.
    /// </summary>
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

    /// <summary>
    /// Elämien määrä, josta visuaalisesti näkee montako elämää on jäljellä.
    /// </summary>
    void LuoElamaLaskuri()
    {
        elamaLaskuri = new DoubleMeter(5);
        elamaLaskuri.MaxValue = 5;
        elamaLaskuri.LowerLimit += ElamaLoppui;

        ProgressBar elamaPalkki = new ProgressBar(150, 30);
        elamaPalkki.X = Screen.Center.X;
        elamaPalkki.Y = Screen.Top - 20;
        elamaPalkki.BindTo(elamaLaskuri);
        elamaPalkki.Image = LoadImage("emptyHearts.png");
        elamaPalkki.BarImage = LoadImage("fullHearts.png");
        Add(elamaPalkki);
    }

    /// <summary>
    /// Kerätessä karkkeja karkkipalkki täyttyy. Kun karkkipalkki on täynnä saa yhden elämän lisää.
    /// </summary>
    void LuoKarkkiLaskuri()
    {
        karkkiLaskuri = new DoubleMeter(0);
        karkkiLaskuri.MaxValue = 5;
        karkkiLaskuri.UpperLimit += ViisiKarkkia;

        ProgressBar karkkiPalkki = new ProgressBar(150, 30);
        karkkiPalkki.X = Screen.Center.X;
        karkkiPalkki.Y = Screen.Top - 60;
        karkkiPalkki.BindTo(karkkiLaskuri);
        karkkiPalkki.Image = LoadImage("tyhjatKarkit.png");
        karkkiPalkki.BarImage = LoadImage("taydetKarkit.png");
        Add(karkkiPalkki);
    }
    void ElamaLoppui()
    {
        ClearAll();
        Valikko();
    }

    /// <summary>
    /// Kerätessä viisi karkkia saa yhden elämän lisää.
    /// </summary>
    void ViisiKarkkia()
    {
        elamaLaskuri.Value += 1;
        karkkiLaskuri.Value = 0;
        karkkiAani.Play();
        MessageDisplay.Add("Sait yhden elämän lisää!");
    }


}