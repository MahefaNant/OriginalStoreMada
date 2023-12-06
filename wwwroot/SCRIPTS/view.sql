


DROP VIEW v_beneficereel;
DROP VIEW v_total_benefice;
DROP VIEW v_total_benefice1;

DROP VIEW v_beneficereel_par_annee;
DROP VIEW v_total_benefice_par_annee;
DROP VIEW v_total_benefice_par_annee1;

DROP VIEW v_beneficereel_par_annee_mois;
DROP VIEW v_total_benefice_par_annee_mois;
DROP VIEW v_total_benefice_par_annee_mois1;
--------------------------------
DROP VIEW v_diversentrer;
DROP VIEW v_diversentrer_par_ans;
DROP VIEW v_diversentrer_par_ansmois;
DROP VIEW v_diversentrer_par_ans_type;
DROP VIEW v_diversentrer_par_ansmois_type;
-------------------------
DROP VIEW v_diversdepense;
DROP VIEW v_diversdepense_par_ans;
DROP VIEW v_diversdepense_par_ansmois;
DROP VIEW v_diversdepense_par_ans_type;
DROP VIEW v_diversdepense_par_ansmois_type;
------------------------
DROP VIEW v_beneficereel_facture;
DROP VIEW v_beneficereel_facture_par_ans;
DROP VIEW v_beneficereel_facture_par_ansmois;
DROP VIEW v_beneficereel_facture_parboutique;
DROP VIEW v_beneficereel_facture_par_ans_parboutique;
DROP VIEW v_beneficereel_facture_par_ansmois_parboutique;
--------------------------
DROP VIEW v_benefice_devis_reel;
DROP VIEW v_benefice_devis_par_ans_reel;
DROP VIEW v_benefice_devis_par_ansmois_reel;
DROP VIEW v_payementdevis_sum;
DROP VIEW v_payementdevis;
DROP VIEW v_info_payementdevis;
DROP VIEW v_payementdevis_payer;
DROP VIEW v_sumcommandedevis_with_fraiscomm;
DROP VIEW v_sumcommandedevis_with_fraiscomm1;
-------------------------------
DROP VIEW v_produiteventreste;
---------------------------

DROP VIEW v_payementproduit;
DROP VIEW v_imagepreference;
DROP VIEW v_facture;
DROP VIEW v_facture1;
DROP VIEW v_payementproduit_sum;
DROP VIEW v_cartproduit;
------------------------------
DROP VIEW v_payementdevis_etat;
DROP VIEW v_payementdevis_etat;
DROP VIEW v_payement_total;
DROP VIEW v_devisinfo_valider;
-----------------------------------

DROP VIEW v_voldevis_Livraison;
DROP VIEW v_commande_devis_reste_invol;
DROP VIEW v_quantiter_commandedevis_invol;
DROP VIEW v_devisinfo;
DROP VIEW v_devisinfo_4;
DROP VIEW v_devisinfo_3;
DROP VIEW v_devisinfo_2;
DROP VIEW v_devisinfo_1;
DROP VIEW v_checkislivrer_all;
DROP VIEW v_relation_quantcommdev_checkAddev;
DROP VIEW v_checkislivrer_inAdressedevis;
DROP VIEW v_quantiter_commandedevis;

-----------------------------------------
DROP VIEW v_stockglobal_parpreference;
DROP VIEW v_stockpreference;
DROP VIEW v_stockproduit_details;
DROP VIEW v_stockproduitboutique_details;
DROP VIEW v_stockproduit_detailsglobal;
DROP VIEW v_stockproduit_detailsglobal2;
DROP VIEW v_stockproduit_detailsglobal1;
DROP VIEW v_stockproduitglobal;
DROP VIEW v_stockproduitglobalBoutique_details;
DROP VIEW v_stockproduitglobalBoutique;
DROP VIEW v_stockproduit;
DROP VIEW v_stocksortie_sumparboutique;
DROP VIEW v_stockentree_sumparboutique;
-----------------------------------------

DROP VIEW v_countfavoris_produit;

-----------------------------------------

DROP VIEW v_countbestseller_par_ans;
DROP VIEW v_countbestseller_par_ans_boutique;
DROP VIEW v_countbestseller_par_ans_mois;
DROP VIEW v_countbestseller_par_ans_mois_boutique;
DROP VIEW v_countbestseller;
DROP VIEW v_countbestseller_parboutique;

-----------------------------------------

DROP VIEW v_payement_produit;
DROP VIEW v_payement_produit1;

-----------------------------------------
DROP VIEW v_imageprincipal_prix_produit;
DROP VIEW v_imageprincipal_prix_produit1;
DROP VIEW v_promotion_encours;
DROP VIEW v_promotion_produit_last;
DROP VIEW v_imageprincipal_prix_produit_withoutprom;
DROP VIEW v_imageprincipalproduit;
DROP VIEW v_produit;

---------------------------------------------
DROP VIEW v_commandedevis;
DROP VIEW v_categorieproduit;
DROP VIEW v_commissionmax;
DROP VIEW v_fraisimportationmax;
DROP VIEW v_infosmax;

---------------------------------------------





        

---------------------------------------------

CREATE OR REPLACE VIEW v_infosmax AS
    SELECT
        COALESCE(MAX(max_frais.montant_euro), 0) AS max_frais_importation,
        COALESCE(MAX(max_comm.montant_euro),0) AS max_commission,
        MAX(max_cours.montant_ariary) AS max_cours_euro
    FROM (
             SELECT montant_euro, datedeb
             FROM frais_importation
             ORDER BY datedeb DESC
                 LIMIT 1
         ) AS max_frais
             FULL JOIN (
        SELECT montant_euro, datedeb
        FROM commission
        ORDER BY datedeb DESC
            LIMIT 1
    ) AS max_comm ON true
             FULL JOIN (
        SELECT montant_ariary, date
        FROM cours_euro
        ORDER BY date DESC
            LIMIT 1
    ) AS max_cours ON true;


---------------------------------------------

CREATE OR REPLACE VIEW v_commissionmax AS
    SELECT c.id_categorie, c.montant_euro, c.datedeb
    FROM commission c
             JOIN (
        SELECT id_categorie, MAX(datedeb) AS datedeb_max
        FROM commission
        GROUP BY id_categorie
    ) subquery
                  ON c.id_categorie = subquery.id_categorie AND c.datedeb = subquery.datedeb_max;

CREATE OR REPLACE VIEW v_fraisimportationmax AS
    SELECT c.id_categorie, c.montant_euro, c.datedeb
    FROM frais_importation c
             JOIN (
        SELECT id_categorie, MAX(datedeb) AS datedeb_max
        FROM frais_importation
        GROUP BY id_categorie
    ) subquery
                  ON c.id_categorie = subquery.id_categorie AND c.datedeb = subquery.datedeb_max;

CREATE OR REPLACE VIEW v_commandedevis AS
    SELECT
        cd.id,
        cd.id_devis,
        cd.id_categorie,
        cd.produit_name,
        cd.prix_euro,
        cd.couleur,
        cd.taille,
        cd.nombre,
        cd.reference_site,
        cd.image,
        cd.frais_importation_reel, 
        cd.commission_reel, 
        COALESCE(vc.montant_euro, 0) AS commission_montant,
        COALESCE(vf.montant_euro, 0) AS frais_importation_montant
    FROM
        commande_devis cd
            LEFT JOIN v_commissionmax vc ON cd.id_categorie = vc.id_categorie
            LEFT JOIN v_fraisimportationmax vf ON vf.id_categorie = vc.id_categorie;

CREATE OR REPLACE VIEW v_categorieproduit AS
    SELECT c.*, com.montant_euro as montant_commission, f.montant_euro as montant_fraisimportation
    FROM categorie_produit c
             LEFT JOIN v_commissionmax AS com ON c.id = com.id_categorie
             LEFT JOIN v_fraisimportationmax AS f ON c.id = f.id_categorie;


---------------------------------------------

CREATE OR REPLACE VIEW v_produit AS
SELECT
    p.id,
    p.id_categorie,
    c.nom as categorie,
    p.nom,
    p.description,
    p.fournisseur,
    p.id_type ,
    t.nom as type ,
    p.pour_enfant
FROM
    produit p
        JOIN
    categorie_produit c ON p.id_categorie = c.id
        JOIN
    type_produit t ON p.id_type = t.id;



CREATE OR REPLACE VIEW v_imageprincipalproduit AS
    SELECT
        p.id AS id_produit,
        p.id_categorie,
        p.categorie,
        p.id_type ,
        p.type,
        p.pour_enfant,
        p.nom,
        p.description,
        p.fournisseur,
        COALESCE(cp.image, NULL) AS image
    FROM
        v_produit p
            LEFT JOIN
        contenue_produit cp ON p.id = cp.id_produit AND cp.isprincipal = true;

CREATE OR REPLACE VIEW v_imageprincipal_prix_produit_withoutprom AS
    SELECT 
        vip.id_produit,
        vip.id_categorie,
        vip.categorie,
        vip.id_type ,
        vip.type,
        vip.pour_enfant,
        vip.nom,
        vip.description,
        vip.fournisseur,
        vip.image,
        COALESCE(pp.prix_vente, NULL) AS prix_vente,
        COALESCE(pp.prix_achat, NULL) AS prix_achat,
        pp.datedeb
    FROM
        v_imageprincipalproduit vip
            LEFT JOIN LATERAL (
            SELECT
                prix_vente, prix_achat, datedeb
            FROM
                prix_produit pp
            WHERE
                    pp.id_produit = vip.id_produit
              AND pp.datefin IS NULL  -- Consider only records without an end date
            ORDER BY
                pp.datedeb DESC
                LIMIT 1
    ) AS pp ON true;

CREATE OR REPLACE VIEW v_promotion_produit_last AS
    SELECT p1.id, p1.id_produit, p1.pourcentage, p1.datedeb, p1.datefin
    FROM promotion_produit p1
             JOIN (
        SELECT id_produit, MAX(datedeb) AS max_datedeb
        FROM promotion_produit
        GROUP BY id_produit
    ) p2 ON p1.id_produit = p2.id_produit AND p1.datedeb = p2.max_datedeb;

CREATE OR REPLACE VIEW v_promotion_encours AS
    SELECT id_produit, pourcentage, MAX(datedeb) AS datedeb, MAX(datefin) AS datefin
    FROM promotion_produit
    WHERE NOW() BETWEEN datedeb AND datefin
    GROUP BY id_produit, pourcentage;



CREATE OR REPLACE VIEW v_imageprincipal_prix_produit1 As
    SELECT
        p.id_produit,
        p.id_categorie,
        p.categorie,
        p.id_type,
        p.type,
        p.pour_enfant,
        p.nom,
        p.description,
        p.fournisseur,
        p.image,
        p.prix_vente AS prix_vente_initial,
        p.prix_achat,
        p.datedeb AS date,
        COALESCE(pr.pourcentage , 0) as pourcentage_promotion,
        COALESCE(pr.datedeb , null) as promo_datedeb,
        COALESCE(pr.datefin , null) as promo_datefin,
        CASE
            WHEN pr.datedeb IS NOT NULL THEN ROUND(p.prix_vente::numeric * (1 - COALESCE(pr.pourcentage, 0) / 100)::numeric, 2)
            ELSE ROUND(p.prix_vente::numeric, 2)
        END AS prix_vente_avec_promotion
    FROM
        v_imageprincipal_prix_produit_withoutprom p
    LEFT JOIN
        v_promotion_encours pr ON p.id_produit = pr.id_produit;
            
CREATE OR REPLACE VIEW v_imageprincipal_prix_produit AS 
    SELECT i.* ,
           COALESCE(p.pourcentage , 0) as pourcentage_avenir,
           COALESCE(p.datedeb , null) as promo_datedeb_avenir ,
           COALESCE(p.datefin , null) as promo_datefin_avenir
    FROM v_imageprincipal_prix_produit1 as i
    LEFT JOIN v_promotion_produit_last as p on i.id_produit = p.id_produit and p.datedeb > now();
        
--------------------------------------------------
        
CREATE OR REPLACE VIEW v_payement_produit1 AS
    SELECT p.* ,
           f.id_boutique,
           f.estpayer
    FROM payement_produit as p
             JOIN facture as f on p.id_facture = f.id ;

CREATE OR REPLACE VIEW v_payement_produit AS
    SELECT pay.* ,
           p.id_produit
    FROM v_payement_produit1 as pay
             JOIN preference_produit as p on pay.id_preferenceproduit = p.id;

--------------------------------------------------------------------------
----------------------------BEST SELLERS--------------------------------
--------------------------------------------------------------------------

CREATE OR REPLACE VIEW v_countbestseller_parboutique AS
    SELECT
        id_boutique,
        id_produit ,
        sum(quantiter) as nombre_seller
    FROM v_payement_produit as pp
    GROUP BY id_boutique,id_produit
    ORDER BY id_boutique,id_produit;

CREATE OR REPLACE VIEW v_countbestseller AS
    SELECT
        id_produit ,
        sum(nombre_seller) as nombre_seller
    FROM v_countbestseller_parboutique as pp
    GROUP BY id_produit
    ORDER BY id_produit;

CREATE OR REPLACE VIEW v_countbestseller_par_ans_mois_boutique AS
    SELECT
        id_boutique,
        id_produit,
        EXTRACT(MONTH FROM date) AS mois,
        EXTRACT(YEAR FROM date) AS annee,
        SUM(quantiter) AS total_quantite
    FROM
        v_payement_produit
    GROUP BY
        id_boutique,id_produit, mois, annee;

CREATE OR REPLACE VIEW v_countbestseller_par_ans_mois AS
    SELECT
        id_produit,
        mois,
        annee,
        SUM(total_quantite) AS total_quantite
    FROM
        v_countbestseller_par_ans_mois_boutique
    GROUP BY
        id_produit, mois, annee;

CREATE OR REPLACE VIEW v_countbestseller_par_ans_boutique AS
    SELECT
        id_boutique ,
        id_produit,
        annee,
        SUM(total_quantite) AS total_quantite
    FROM
        v_countbestseller_par_ans_mois_boutique
    GROUP BY
        id_boutique ,id_produit, annee;

CREATE OR REPLACE VIEW v_countbestseller_par_ans AS
    SELECT
        id_produit,
        annee,
        SUM(total_quantite) AS total_quantite
    FROM
        v_countbestseller_par_ans_boutique
    GROUP BY
        id_produit, annee;



--------------------------------------------------------------------------
----------------------------favoris_produit--------------------------------
--------------------------------------------------------------------------

CREATE OR REPLACE VIEW v_countfavoris_produit AS
SELECT
    id_produit ,
    count(id_produit) as nombre_favoris
FROM favoris_produit as pp
GROUP BY id_produit
ORDER BY id_produit;


----------------------STOCK-------------------------------

CREATE OR REPLACE VIEW v_stockentree_sumparboutique AS
    SELECT
        id_boutique,
        id_produit,
        id_preferenceproduit,
        SUM(quantiter) AS total_quantities
    FROM
        entree_produit
    GROUP BY
        id_boutique,id_produit, id_preferenceproduit;

CREATE OR REPLACE VIEW v_stocksortie_sumparboutique AS
    SELECT
        id_boutique,
        id_produit,
        id_preferenceproduit,
        SUM(quantiter) AS total_quantities
    FROM 
        sortie_produit
    GROUP BY
        id_boutique,id_produit, id_preferenceproduit;

CREATE OR REPLACE VIEW v_stockproduit AS
    SELECT
        e.id_boutique,
        e.id_produit,
        e.id_preferenceproduit,
        COALESCE(e.total_quantities, 0) - COALESCE(s.total_quantities, 0) AS stock
    FROM
        v_stockentree_sumparboutique e
            LEFT JOIN
        v_stocksortie_sumparboutique s ON e.id_boutique = s.id_boutique AND e.id_produit = s.id_produit AND e.id_preferenceproduit = s.id_preferenceproduit;

CREATE OR REPLACE VIEW v_stockproduitglobalBoutique AS 
    SELECT id_boutique,
           id_produit, 
           SUM(stock) as stock
    FROM v_stockproduit
    GROUP BY id_boutique, id_produit;

CREATE OR REPLACE VIEW v_stockproduitglobalBoutique_details AS 
    SELECT s.* ,
           b.longitude,b.latitude
    FROM v_stockproduitglobalBoutique s
    JOIN boutique b on s.id_boutique = b.id;

CREATE OR REPLACE VIEW v_stockproduitglobal AS
SELECT
       id_produit,
       SUM(stock) as stock
FROM v_stockproduit
GROUP BY id_produit;
    

CREATE OR REPLACE VIEW v_stockproduit_detailsglobal1 AS
    SELECT
        s.id_produit,s.stock,
        p.id_categorie,
        p.categorie,
        p.id_type,
        p.type,
        p.pour_enfant,
        p.nom,
        p.description,
        p.fournisseur,
        p.image,
        p.prix_vente_initial,
        p.prix_achat,
        p.date ,
        p.pourcentage_promotion,
        p.promo_datedeb,
        p.promo_datefin,
        p.prix_vente_avec_promotion
    FROM
        v_stockproduitGlobal s
            JOIN
        v_imageprincipal_prix_produit p ON s.id_produit = p.id_produit;

CREATE OR REPLACE VIEW v_stockproduit_detailsglobal2 AS
    SELECT s.*,
           COALESCE(c.nombre_seller , 0) as nombre_seller
    FROM v_stockproduit_detailsglobal1 as s
             LEFT JOIN v_countbestseller as c on s.id_produit = c.id_produit;

CREATE OR REPLACE VIEW v_stockproduit_detailsglobal AS
    SELECT s.*,
           COALESCE(c.nombre_favoris , 0) as nombre_favoris
    FROM v_stockproduit_detailsglobal2 as s
             LEFT JOIN v_countfavoris_produit as c on s.id_produit = c.id_produit;

CREATE OR REPLACE VIEW v_stockproduitboutique_details AS
SELECT
    v.id_boutique,
    v.id_produit,
    v.id_preferenceproduit,
    v.stock,
    p.taille,
    p.id_contenue
FROM
    v_stockproduit AS v
        JOIN
    preference_produit AS p
    ON
            v.id_preferenceproduit = p.id;

CREATE OR REPLACE VIEW v_stockproduit_details AS
    SELECT
        id_produit,
        id_preferenceproduit,
        SUM(stock) as stock,
        taille,
        id_contenue
    FROM
        v_stockproduitboutique_details
    GROUP BY id_produit, id_preferenceproduit,taille, id_contenue ;



-----------------------------------------------------

CREATE OR REPLACE VIEW v_stockpreference AS
    SELECT
        p.id_produit,
        p.taille,
        p.id_contenue,
        p.id as id_preferenceproduit,
        COALESCE(v.id_boutique, null) as id_boutique,
        COALESCE(v.stock, null) as stock
    FROM
        preference_produit AS p
        LEFT JOIN v_stockproduit AS v on p.id = v.id_preferenceproduit;

CREATE OR REPLACE VIEW v_stockglobal_parpreference AS 
    SELECT id_produit, taille, id_contenue, id_preferenceproduit ,
           sum(COALESCE(stock , 0)) as stock
    FROM v_stockpreference
    GROUP BY id_contenue , id_preferenceproduit,taille , id_produit ;

------------------------------------------------------------
--------------------------DEVIS----------------------------------
------------------------------------------------------------


CREATE OR REPLACE VIEW v_quantiter_commandedevis AS
    SELECT id_devis, 
           SUM( nombre ) as nombre
    FROM commande_devis
    GROUP BY id_devis;

CREATE OR REPLACE VIEW v_checkislivrer_inAdressedevis AS
SELECT
    cd.id_devis,
    sum( COALESCE(quantiter , 0) ) as quantiter ,
    CASE
        WHEN COUNT(*) FILTER (WHERE cd.date_livrer IS NULL) >= 1 THEN FALSE
        ELSE TRUE
        END AS is_livrer
FROM adresse_livraisondevis cd
GROUP BY cd.id_devis;

CREATE OR REPLACE VIEW v_relation_quantcommdev_checkAddev AS
    SELECT q.id_devis,
           q.nombre as nombre_comm,
           COALESCE(a.quantiter  , 0)as quantiter_adr ,
           q.nombre - COALESCE(a.quantiter,0) as reste ,
           a.is_livrer
    FROM v_quantiter_commandedevis as q 
    LEFT JOIN v_checkislivrer_inAdressedevis as a ON q.id_devis = a.id_devis;


CREATE OR REPLACE VIEW v_checkislivrer_all AS
    SELECT id_devis,
       CASE
           WHEN bool_or(is_livrer = TRUE AND reste = 0) THEN TRUE
           ELSE FALSE
           END AS is_livrer
    FROM v_relation_quantcommdev_checkAddev
    GROUP BY id_devis;



CREATE OR REPLACE VIEW v_devisinfo_1 AS
SELECT
    d.id AS id_devis,
    d.id_client,
    d.date,
    ROUND(SUM(COALESCE(cd.prix_euro * cd.nombre, 0)), 2) AS total_prixeuro,
    ROUND(SUM(COALESCE(cd.nombre, 0)), 2) AS total_quantiter,
    ROUND(SUM(COALESCE(cd.frais_importation_montant * cd.nombre, 0)), 2) AS total_frais_importation,
    ROUND(SUM(COALESCE(cd.commission_montant * cd.nombre, 0)), 2) AS total_commission,
    ROUND(SUM(COALESCE(cd.frais_importation_reel * cd.nombre, 0)), 2) AS total_frais_importation_reel,
    ROUND(SUM(COALESCE(cd.commission_reel * cd.nombre, 0)), 2) AS total_commission_reel,
    ROUND(d.cours_devis, 2) AS cours_devis,
    d.date_envoi,
    d.date_validation,
    d.date_delete,
    d.date_payer,
    d.remarque
FROM
    devis d
        LEFT JOIN
    v_commandedevis cd ON cd.id_devis = d.id
GROUP BY
    d.id,
    d.id_client,
    d.date,
    d.date_envoi,
    d.date_validation,
    d.date_delete,
    d.date_payer,
    d.cours_devis, d.remarque ;


CREATE OR REPLACE VIEW v_devisinfo_2 AS
    SELECT
        d.id_devis,
        d.id_client,
        d.date,
        d.total_prixeuro,
        d.total_quantiter,
        d.total_frais_importation,
        d.total_commission,
        d.total_frais_importation_reel,
        d.total_commission_reel,
        d.cours_devis,
        d.date_envoi,
        d.date_validation,
        d.date_delete,
        d.date_payer,
        c.montant_ariary AS cours_euro,
        d.remarque
    FROM
        v_devisinfo_1 d
            CROSS JOIN (
            SELECT montant_ariary
            FROM cours_euro
            ORDER BY date DESC
                LIMIT 1
        ) c;

CREATE OR REPLACE VIEW v_devisinfo_3 AS
    SELECT
        id_devis,
        id_client,
        date,
        total_prixeuro,
        Round(total_prixeuro + total_frais_importation + total_commission ,2) AS total_prixfin_euro,
        CASE
            WHEN cours_devis IS NOT NULL THEN Round(total_prixeuro * cours_devis, 2)
            ELSE 0
        END AS total_prixariary_reel,
        CASE
            WHEN cours_devis IS NOT NULL THEN Round(total_prixeuro * cours_devis + cours_devis * (total_frais_importation_reel + total_commission_reel) ,2 )
            ELSE 0
        END AS total_prixfinariary_reel,
        Round(total_prixeuro * cours_euro , 2) AS total_prixariary_1,
        Round(total_prixeuro * cours_euro +  cours_euro * (total_frais_importation + total_commission) , 2) AS total_prixfinariary_1,
        total_quantiter,
        total_frais_importation,
        ROUND(total_frais_importation * cours_euro,2) AS total_frais_importation_ariary,
        total_commission,
        ROUND(total_commission * cours_euro,2) AS total_commission_ariary,
        total_frais_importation_reel,
        ROUND(total_frais_importation_reel * cours_devis,2) AS total_frais_importation_reelariary,
        total_commission_reel,
        ROUND(total_frais_importation_reel * cours_devis,2) AS total_commission_reelariary,
        cours_devis,
        date_envoi,
        date_validation,
        date_delete,
        date_payer,
        cours_euro,
        remarque
    FROM v_devisinfo_2;
            
CREATE OR REPLACE VIEW v_devisinfo_4 AS
    SELECT d.*,
           COALESCE(vd.is_livrer, false) as is_livrer
    FROM v_devisinfo_3 d
             LEFT JOIN v_checkislivrer_all vd on d.id_devis = vd.id_devis;

CREATE OR REPLACE VIEW v_devisinfo AS
    SELECT d.*,
           CASE
               WHEN d.id_devis IN (SELECT id_devis FROM payement_devis WHERE datepayement IS NULL LIMIT 1) THEN false
               ELSE true
    END AS etatPayement
    FROM v_devisinfo_4 d
             LEFT JOIN payement_devis pd ON d.id_devis = pd.id_devis
        where d.total_prixeuro > 0
    GROUP BY d.id_devis, d.id_client, d.date, d.total_prixeuro, d.total_prixfin_euro, d.total_prixariary_reel, d.total_prixfinariary_reel,
             d.total_prixariary_1, d.total_prixfinariary_1, d.total_quantiter, d.total_frais_importation, d.total_frais_importation_ariary,
             d.total_commission, d.total_commission_ariary, d.total_frais_importation_reel, d.total_frais_importation_reelariary,
             d.total_commission_reel, d.total_commission_reelariary, d.cours_devis, d.date_envoi, d.date_validation, d.date_delete,
              d.date_payer, d.cours_euro,
             d.remarque, d.is_livrer;
        
        
CREATE OR REPLACE VIEW v_quantiter_commandedevis_invol AS 
    SELECT
        id_commande ,
        sum(quantiter) as quantiter_utiliser
    FROM vol_devis
    GROUP BY id_commande ;
        
CREATE OR REPLACE VIEW v_commande_devis_reste_invol AS 
    SELECT 
        c.* ,
        COALESCE(q.quantiter_utiliser , 0) as quantiter_utiliser,
        c.nombre - COALESCE(q.quantiter_utiliser , 0) as quantiter_reste
    FROM commande_devis as c
    LEFT JOIN v_quantiter_commandedevis_invol as q on c.id = q.id_commande;

CREATE OR REPLACE VIEW v_voldevis_livraison AS 
    SELECT v.* ,
           a.date_livrer ,
           a.ville ,a.quartier, a.longitude, a.latitude,a.isinboutique,a.fraislivraison,a.datepret, a.dateestim,
           CASE 
               WHEN a.id is not null and a.id > 0 THEN true
            ELSE FALSE 
           END as in_adresse
    FROM vol_devis as v
    LEFT JOIN adresse_livraisondevis as a on v.id = a.id_voldevis;
        
/*--------------------------PAYEMENT DEVIS-------------------------------------*/
       
CREATE OR REPLACE VIEW v_devisinfo_valider AS
    SELECT id_devis, id_client, date_payer ,
           CASE WHEN cours_devis IS NOT NULL THEN total_prixariary_reel ELSE total_prixariary_1 END AS total_prixariary_reel,
           CASE WHEN cours_devis IS NOT NULL THEN total_prixfinariary_reel ELSE total_prixfinariary_1 END AS total_prixfinariary_reel,
           total_quantiter
    FROM v_devisinfo;
            
CREATE OR REPLACE VIEW v_payement_total AS 
    SELECT 
        id_devis , id_client, ROUND( SUM(montant) ,2) as montant_payer
    FROM payement_devis
    GROUP BY id_devis , id_client;

CREATE OR REPLACE VIEW v_payementdevis_etat AS
    SELECT
        dv.id_devis,
        dv.id_client,
        dv.total_prixariary_reel,
        dv.total_prixfinariary_reel,
        dv.total_quantiter,
        COALESCE(pt.montant_payer, 0) AS montant_payer,
        CASE WHEN dv.date_payer IS NOT NULL THEN true ELSE false END AS isPayer,
        CASE WHEN dv.total_prixfinariary_reel = 0
                 THEN 0
             ELSE Round((COALESCE(pt.montant_payer, 0) / dv.total_prixfinariary_reel) * 100 ,2)
            END AS pourcentage_payer,
        ROUND(COALESCE(dv.total_prixfinariary_reel,0) - COALESCE(pt.montant_payer, 0) ,2) AS reste_apayer
    FROM v_devisinfo_valider dv
             LEFT JOIN v_payement_total pt ON dv.id_devis = pt.id_devis AND dv.id_client = pt.id_client;

            
/*-----------------------------------------------------------------------------*/
/*-----------------------------SHOP BOUTIQUE------------------------------------------------*/
/*-----------------------------------------------------------------------------*/

/*-----------------------------------------------------------------------------*/
CREATE OR REPLACE VIEW v_cartproduit AS
    SELECT pp.id AS id_preferenceproduit, pp.taille, pp.id_contenue, vippp.*
    FROM preference_produit AS pp
             INNER JOIN v_imageprincipal_prix_produit AS vippp ON pp.id_produit = vippp.id_produit;

CREATE OR REPLACE VIEW v_payementproduit_sum AS
    SELECT id_facture,
           id_typepayement ,
           sum(quantiter) as quantiter_total,
           Round( sum(montant * quantiter) ,2) as montant_sum 
    FROM payement_produit
    GROUP BY id_facture, id_typepayement;

CREATE OR REPLACE VIEW v_facture1 AS 
    SELECT f.id as id_facture, f.id_client, f.id_boutique, f.date,f.date_livrer,f.estpayer,f.datesuspendue,
           p.quantiter_total, p.montant_sum , p.id_typepayement
    FROM facture as f JOIN v_payementproduit_sum as p on f.id = p.id_facture;

CREATE OR REPLACE VIEW v_facture AS
    SELECT f.*,
           ROUND( f.montant_sum + a.fraislivraison ,2) as montant_fin,
           a.ville as ville_adresse, a.quartier as quartier_adresse, a.longitude as longitude_adresse, a.latitude as latitude_adresse,
           a.isinboutique, a.fraislivraison, a.datepret, a.dateestim
    FROM v_facture1 as f Join adresse_livraisonfacture as a on f.id_facture = a.id_facture;

CREATE OR REPLACE VIEW v_imagepreference AS 
    SELECT p.*, cp.image 
    FROM preference_produit as p
     LEFT JOIN
     contenue_produit cp ON p.id_produit = cp.id_produit AND cp.isprincipal = true; 

CREATE OR REPLACE VIEW v_payementproduit AS 
    SELECT p.*, prod.image 
    FROM payement_produit as p
    LEFT JOIN v_imagepreference as prod on p.id_preferenceproduit = prod.id;


--------------------------------------------------------------------------
----------------------EVENEMENT----------------------------------------------------
--------------------------------------------------------------------------

CREATE OR REPLACE VIEW v_produiteventreste AS
    SELECT p.id as id_produitevent , p.id_evenement, p.id_categorie, p.produit_name, p.prix,p.couleur,p.taille, p.reference_site, p.image,
           (p.quantitermax - sum(COALESCE(i.quantiter,0))) as quantiter_reste, 
           p.quantitermax as quantiter_initiale
    FROM produit_event as p 
    LEFT JOIN interaction_event as i on p.id = i.id_produitevent
    GROUP BY p.id, p.id_evenement;
--------------------------------------------------------------------------

--------------------------------------------------------------------------
-----------------------------STAT ADMIN--------------------------------
--------------------------------------------------------------------------

-------------------DEVIS-------------------


CREATE OR REPLACE VIEW v_sumcommandedevis_with_fraiscomm1 AS 
    SELECT id_devis ,
           ROUND(sum(prix_euro * nombre) ,2) as total_prixeuro ,
           ROUND(sum(frais_importation_reel * nombre) ,2) as frais_importation ,
           ROUND(sum(commission_reel * nombre) ,2) as commission ,
           sum(nombre) as quantiter
    FROM commande_devis
    GROUP BY id_devis;

CREATE OR REPLACE VIEW v_sumcommandedevis_with_fraiscomm AS  
    SELECT v.id_devis,
           ROUND(v.total_prixeuro * d.cours_devis ,2) as total_prixar,
           ROUND(v.frais_importation * d.cours_devis,2) as frais_importation,
           ROUND(v.commission * d.cours_devis ,2) as commission 
    FROM v_sumcommandedevis_with_fraiscomm1  as v
    join devis as d on v.id_devis = d.id where cours_devis is not null
    and frais_importation is not null and commission is not null;

CREATE OR REPLACE VIEW v_payementdevis_payer AS
    SELECT id_devis ,
           ROUND(sum(montant) ,2) as total_prix
    FROM payement_devis
    WHERE datepayement is not null
    GROUP BY id_devis;

CREATE OR REPLACE VIEW v_info_payementdevis AS
    SELECT id_devis,total_prixar , frais_importation, commission ,
           ROUND(total_prixar + frais_importation + commission ,2 ) as total_prixreel
    FROM v_sumcommandedevis_with_fraiscomm;
           

CREATE OR REPLACE VIEW v_payementdevis AS 
    SELECT p.* ,
           i.total_prixreel, frais_importation, commission,
           ROUND( (p.montant * i.commission)/i.total_prixreel ,2) as benefice
    FROM payement_devis as p
    JOIN v_info_payementdevis as i on p.id_devis = i.id_devis where datepayement is not null;

CREATE OR REPLACE VIEW v_payementdevis_sum AS
    select id_devis,id_client,
           round(sum(montant),2) as montant, id_typepayement , date, datepayement , 
        round(sum(total_prixreel),2) as total_prixreel , round(sum(frais_importation),2)  as frais_importation ,
        round(sum(commission),2)  as commission , round(sum(benefice),2) as benefice 
    from v_payementdevis group by id_devis, id_client, id_typepayement,date, datepayement;

CREATE OR REPLACE VIEW v_benefice_devis_par_ansmois_reel AS
    SELECT
        EXTRACT(YEAR FROM datepayement) AS annee,
        EXTRACT(MONTH FROM datepayement) AS mois,
        ROUND(SUM(benefice) ,2) AS total_benefice
    FROM v_payementdevis
    GROUP BY annee, mois
    ORDER BY annee, mois;

CREATE OR REPLACE VIEW v_benefice_devis_par_ans_reel AS
    SELECT
        EXTRACT(YEAR FROM datepayement) AS annee,
        ROUND(SUM(benefice) ,2) AS total_benefice
    FROM v_payementdevis
    GROUP BY annee
    ORDER BY annee;

CREATE OR REPLACE VIEW v_benefice_devis_reel AS
    SELECT
        ROUND(SUM(benefice) ,2) AS total_benefice
    FROM v_payementdevis;


-------------------------------------------
--------------FACTURE-----------------------------

CREATE OR REPLACE VIEW v_beneficereel_facture_par_ansmois_parboutique AS
    SELECT
        id_boutique ,
        EXTRACT(YEAR FROM date) AS annee,
        EXTRACT(MONTH FROM date) AS mois,
        ROUND(SUM((montant - prix_achat) * quantiter) , 2) AS total_benefice
    FROM v_payement_produit
    WHERE estpayer is not null 
    GROUP BY id_boutique,annee, mois
    ORDER BY id_boutique,annee, mois;

CREATE OR REPLACE VIEW v_beneficereel_facture_par_ans_parboutique AS
    SELECT
        id_boutique ,
        EXTRACT(YEAR FROM date) AS annee,
        ROUND(SUM((montant - prix_achat) * quantiter) , 2) AS total_benefice
    FROM v_payement_produit
    WHERE estpayer is not null
    GROUP BY id_boutique,annee
    ORDER BY id_boutique,annee;

CREATE OR REPLACE VIEW v_beneficereel_facture_parboutique AS
    SELECT
        id_boutique ,
        ROUND(SUM((montant - prix_achat) * quantiter) , 2) AS total_benefice
    FROM v_payement_produit
    WHERE estpayer is not null
    GROUP BY id_boutique
    ORDER BY id_boutique;


CREATE OR REPLACE VIEW v_beneficereel_facture_par_ansmois AS 
    SELECT annee,mois,
           ROUND(SUM(total_benefice) , 2) AS total_benefice
   FROM v_beneficereel_facture_par_ansmois_parboutique
   GROUP BY annee, mois
   ORDER BY annee, mois;

CREATE OR REPLACE VIEW v_beneficereel_facture_par_ans AS
    SELECT annee,
           ROUND(SUM(total_benefice) , 2) AS total_benefice
    FROM v_beneficereel_facture_par_ansmois_parboutique
    GROUP BY annee
    ORDER BY annee;

CREATE OR REPLACE VIEW v_beneficereel_facture AS
    SELECT
           ROUND(SUM(total_benefice) , 2) AS total_benefice
    FROM v_beneficereel_facture_par_ansmois_parboutique;

--------------------DIVERS DEPENSE--------------------------------------

CREATE OR REPLACE VIEW v_diversdepense_par_ansmois_type AS
    SELECT
        id_typedepense ,
        EXTRACT(YEAR FROM date) AS annee,
        EXTRACT(MONTH FROM date) AS mois,
        ROUND(SUM(montant_ar) , 2) as total_montant
    FROM divers_depense
    GROUP BY id_typedepense, annee, mois
    ORDER BY id_typedepense, annee, mois;

CREATE OR REPLACE VIEW v_diversdepense_par_ans_type AS
    SELECT
        id_typedepense ,
        annee,
        ROUND(SUM(total_montant) , 2) as total_montant
    FROM v_diversdepense_par_ansmois_type
    GROUP BY id_typedepense, annee
    ORDER BY id_typedepense, annee;

CREATE OR REPLACE VIEW v_diversdepense_par_ansmois AS 
    SELECT 
        EXTRACT(YEAR FROM date) AS annee,
        EXTRACT(MONTH FROM date) AS mois, 
        ROUND(SUM(montant_ar) , 2) as total_montant
    FROM divers_depense
    GROUP BY annee, mois
    ORDER BY annee, mois;

CREATE OR REPLACE VIEW v_diversdepense_par_ans AS
    SELECT
        EXTRACT(YEAR FROM date) AS annee,
        ROUND(SUM(montant_ar) , 2) as total_montant
    FROM divers_depense
    GROUP BY annee
    ORDER BY annee;

CREATE OR REPLACE VIEW v_diversdepense AS
    SELECT
        ROUND(SUM(montant_ar) , 2) as total_montant
    FROM divers_depense;

--------------------DIVERS ENTRER--------------------------------------
CREATE OR REPLACE VIEW v_diversentrer_par_ansmois_type AS
    SELECT
        id_typeentrer ,
        EXTRACT(YEAR FROM date) AS annee,
        EXTRACT(MONTH FROM date) AS mois,
        ROUND(SUM(montant_ar) , 2) as total_benefice
    FROM divers_entrer
    GROUP BY id_typeentrer, annee, mois
    ORDER BY id_typeentrer, annee, mois;

CREATE OR REPLACE VIEW v_diversentrer_par_ans_type AS
    SELECT
        id_typeentrer ,
        annee,
        ROUND(SUM(total_benefice) , 2) as total_benefice
    FROM v_diversentrer_par_ansmois_type
    GROUP BY id_typeentrer, annee
    ORDER BY id_typeentrer, annee;

CREATE OR REPLACE VIEW v_diversentrer_par_ansmois AS
    SELECT
        EXTRACT(YEAR FROM date) AS annee,
        EXTRACT(MONTH FROM date) AS mois,
        ROUND(SUM(montant_ar) , 2) as total_benefice
    FROM divers_entrer
    GROUP BY annee, mois
    ORDER BY annee, mois;

CREATE OR REPLACE VIEW v_diversentrer_par_ans AS
    SELECT
        EXTRACT(YEAR FROM date) AS annee,
        ROUND(SUM(montant_ar) , 2) as total_benefice
    FROM divers_entrer
    GROUP BY annee
    ORDER BY annee;

CREATE OR REPLACE VIEW v_diversentrer AS
    SELECT
        ROUND(SUM(montant_ar) , 2) as total_benefice
    FROM divers_entrer;


--------------------BEnefice Total--------------------------------------

CREATE OR REPLACE VIEW v_total_benefice_par_annee_mois1 AS
    SELECT COALESCE(f.annee, d.annee) AS annee,
           COALESCE(f.mois, d.mois) AS mois,
           COALESCE(f.total_benefice, 0) as benefice_facture ,
           COALESCE(d.total_benefice, 0) as benefice_devis ,
           ROUND(COALESCE(f.total_benefice, 0) + COALESCE(d.total_benefice, 0) ,2)AS total_benefice
    FROM v_beneficereel_facture_par_ansmois f
             FULL JOIN v_benefice_devis_par_ansmois_reel d
                       ON f.annee = d.annee AND f.mois = d.mois;

CREATE OR REPLACE VIEW v_total_benefice_par_annee_mois AS
    SELECT COALESCE(tb.annee, de.annee) AS annee,
           COALESCE(tb.mois, de.mois) AS mois,
           COALESCE(tb.benefice_facture, 0) as benefice_facture,
           COALESCE(tb.benefice_devis , 0) as benefice_devis,
           COALESCE(de.total_benefice,0) as benefice_divers ,
           ROUND(COALESCE(tb.total_benefice, 0) + COALESCE(de.total_benefice, 0) ,2 ) AS total_benefice
    FROM v_total_benefice_par_annee_mois1 tb
             FULL JOIN v_diversentrer_par_ansmois de
                       ON tb.annee = de.annee AND tb.mois = de.mois;

CREATE OR REPLACE VIEW v_beneficereel_par_annee_mois AS
    SELECT COALESCE(tb.annee, dd.annee) AS annee,
           COALESCE(tb.mois, dd.mois) AS mois,
           COALESCE(tb.benefice_facture , 0) as recette_facture,
           COALESCE(tb.benefice_devis, 0) as recette_devis,
            COALESCE(tb.benefice_divers , 0) as recette_divers,
           COALESCE(tb.total_benefice, 0) as recette_total,
           COALESCE(dd.total_montant , 0) as depense_total,
           ROUND(  COALESCE(tb.total_benefice, 0) - COALESCE(dd.total_montant , 0) , 2) as benefice_reel
    FROM v_total_benefice_par_annee_mois as tb
        FULL JOIN  v_diversdepense_par_ansmois as dd
            ON tb.annee = dd.annee AND tb.mois = dd.mois;
           

--------

CREATE OR REPLACE VIEW v_total_benefice_par_annee1 AS
    SELECT COALESCE(f.annee, d.annee) AS annee,
           COALESCE(f.total_benefice, 0) as benefice_facture ,
           COALESCE(d.total_benefice, 0) as benefice_devis ,
           ROUND(COALESCE(f.total_benefice, 0) + COALESCE(d.total_benefice, 0) ,2 ) AS total_benefice
    FROM v_beneficereel_facture_par_ans f
             FULL JOIN v_benefice_devis_par_ans_reel d
                       ON f.annee = d.annee;

CREATE OR REPLACE VIEW v_total_benefice_par_annee AS
    SELECT COALESCE(tb.annee, de.annee) AS annee,
           COALESCE(tb.benefice_facture , 0) as benefice_facture,
           COALESCE(tb.benefice_devis , 0) as benefice_devis,
           COALESCE(de.total_benefice,0) as benefice_divers ,
           ROUND(COALESCE(tb.total_benefice, 0) + COALESCE(de.total_benefice, 0) ,2 ) AS total_benefice
    FROM v_total_benefice_par_annee1 tb
             FULL JOIN v_diversentrer_par_ans de
                       ON tb.annee = de.annee;

CREATE OR REPLACE VIEW v_beneficereel_par_annee AS
    SELECT COALESCE(tb.annee, dd.annee) AS annee,
           COALESCE(tb.benefice_facture , 0) as recette_facture,
           COALESCE(tb.benefice_devis , 0) as recette_devis,
           COALESCE(tb.benefice_divers , 0) as recette_divers,
           COALESCE(tb.total_benefice , 0) as recette_total,
           COALESCE(dd.total_montant , 0) as depense_total,
           ROUND(  COALESCE(tb.total_benefice,0) - COALESCE(dd.total_montant , 0) , 2) as benefice_reel
    FROM v_total_benefice_par_annee as tb
             FULL JOIN  v_diversdepense_par_ans as dd
                        ON tb.annee = dd.annee;

--------

CREATE OR REPLACE VIEW v_total_benefice1 AS
    SELECT
        COALESCE(f.total_benefice, 0) as benefice_facture ,
        COALESCE(d.total_benefice, 0) as benefice_devis ,
        ROUND(COALESCE(f.total_benefice, 0) + COALESCE(d.total_benefice, 0) ,2 ) AS total_benefice
    FROM v_beneficereel_facture f
             FULL JOIN v_benefice_devis_reel d
                       ON 1 = 1;

CREATE OR REPLACE VIEW v_total_benefice AS
    SELECT
        COALESCE(tb.benefice_facture, 0) as benefice_facture,
        COALESCE(tb.benefice_devis , 0) as benefice_devis,
        COALESCE(de.total_benefice,0) as benefice_divers ,
        ROUND(COALESCE(tb.total_benefice, 0) + COALESCE(de.total_benefice, 0) ,2 ) AS total_benefice
    FROM v_total_benefice1 tb
             FULL JOIN v_diversentrer de
                       ON 1 = 1;

CREATE OR REPLACE VIEW v_beneficereel AS
    SELECT
           COALESCE(tb.benefice_facture, 0) as recette_facture,
           COALESCE(tb.benefice_devis , 0) as recette_devis,
           COALESCE(tb.benefice_divers,0) as recette_divers,
           COALESCE(tb.total_benefice , 0) as recette_total,
           COALESCE(dd.total_montant , 0) as depense_total,
           ROUND(  COALESCE(tb.total_benefice,0) - COALESCE(dd.total_montant , 0) , 2) as benefice_reel
    FROM v_total_benefice_par_annee as tb
             FULL JOIN  v_diversdepense_par_ans as dd
                        ON 1=1;

-------------------STAT VENTE---------------------------------
